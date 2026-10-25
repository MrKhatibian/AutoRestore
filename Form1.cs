using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    /// <summary>
    /// Finds today's latest .bak file in the source folder, copies it to the
    /// SQL Server backup folder and restores it over the target database.
    /// </summary>
    public partial class AutoRestore : Form
    {
        #region Settings
        private const string DatabaseName = "WebMarket";
        private const string SourceFolderPath = @"F:\OHM\Projeh\AutoMirorring\SQLBackup\";
        private const string DestinationFolderPath = @"C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\Backup\";
        private const string ConnectionString = "Data Source=.;Initial Catalog=master;Integrated Security=SSPI;";
        private const int CommandTimeoutSeconds = 600; // 10 minutes
        #endregion

        public AutoRestore()
        {
            InitializeComponent();
        }

        private async void frmAutoRestore_Load(object sender, EventArgs e)
        {
            try
            {
                // Culture-independent date, e.g. 2025_01_31
                string todayDate = DateTime.Today.ToString("yyyy_MM_dd", CultureInfo.InvariantCulture);

                if (!Directory.Exists(SourceFolderPath))
                {
                    SetStatus("Source path is not accessible.");
                    return;
                }
                if (!Directory.Exists(DestinationFolderPath))
                {
                    SetStatus("Destination path does not exist.");
                    return;
                }

                List<string> sourceFiles = FindBackupFiles(SourceFolderPath, SearchOption.AllDirectories);
                if (sourceFiles.Count == 0)
                {
                    SetStatus("No backup file in source path.");
                    return;
                }

                string latestBackup = GetLatestBackupOfDay(sourceFiles, todayDate);
                if (latestBackup == null)
                {
                    SetStatus("No up-to-date (today's) backup file in source path.");
                    return;
                }

                SetStatus("Preparing destination folder...");
                string destinationFile = PrepareDestination(latestBackup);

                SetStatus("Restoring " + DatabaseName + " ...");
                await RestoreDatabaseAsync(destinationFile);

                SetStatus("DB " + DatabaseName + " restored successfully from " + Path.GetFileName(destinationFile));
            }
            catch (Exception ex)
            {
                SetStatus("Error: " + ex.Message);
                Console.WriteLine(ex);
            }

            // Close(); // uncomment if the app should exit automatically (e.g. scheduled task)
        }

        private void SetStatus(string text)
        {
            lblStatusInfo.Text = text;
            lblStatusInfo.Refresh();
        }

        /// <summary>Returns full paths of all "{DatabaseName}*.bak" files in a folder.</summary>
        private static List<string> FindBackupFiles(string folder, SearchOption option)
        {
            return Directory.GetFiles(folder, DatabaseName + "*.bak", option).ToList();
        }

        /// <summary>
        /// Among the files whose name contains the given date, returns the most
        /// recently written one (or null if there is none).
        /// </summary>
        private static string GetLatestBackupOfDay(IEnumerable<string> files, string dateText)
        {
            return files
                .Where(f => Path.GetFileName(f).Contains(dateText))
                .OrderByDescending(f => File.GetLastWriteTime(f))
                .FirstOrDefault();
        }

        /// <summary>
        /// Removes other backups of this database from the destination folder
        /// (top directory only) and copies the chosen backup there if needed.
        /// Returns the full path of the file in the destination.
        /// </summary>
        private static string PrepareDestination(string sourceFile)
        {
            string fileName = Path.GetFileName(sourceFile);
            string destinationFile = Path.Combine(DestinationFolderPath, fileName);

            foreach (string oldFile in FindBackupFiles(DestinationFolderPath, SearchOption.TopDirectoryOnly))
            {
                if (!string.Equals(Path.GetFileName(oldFile), fileName, StringComparison.OrdinalIgnoreCase))
                    File.Delete(oldFile);
            }

            if (!File.Exists(destinationFile))
                File.Copy(sourceFile, destinationFile);

            return destinationFile;
        }

        /// <summary>Restores the database from the given .bak file (overwrites existing DB).</summary>
        private static async Task RestoreDatabaseAsync(string backupFilePath)
        {
            string quotedDb = "[" + DatabaseName.Replace("]", "]]") + "]";

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                try
                {
                    if (await DatabaseExistsAsync(connection))
                    {
                        // Kick out every active connection
                        await ExecuteAsync(connection,
                            "ALTER DATABASE " + quotedDb + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE");
                    }

                    // The file path is passed as a parameter (no string concatenation)
                    await ExecuteAsync(connection,
                        "RESTORE DATABASE " + quotedDb + " FROM DISK = @path WITH REPLACE",
                        new SqlParameter("@path", backupFilePath));
                }
                finally
                {
                    // Always give the database back to users, even if the restore failed
                    if (await DatabaseExistsAsync(connection))
                    {
                        await ExecuteAsync(connection,
                            "ALTER DATABASE " + quotedDb + " SET MULTI_USER");
                    }
                }
            }
        }

        private static async Task<bool> DatabaseExistsAsync(SqlConnection connection)
        {
            using (var cmd = new SqlCommand("SELECT DB_ID(@name)", connection))
            {
                cmd.Parameters.AddWithValue("@name", DatabaseName);
                object result = await cmd.ExecuteScalarAsync();
                return result != null && result != DBNull.Value;
            }
        }

        private static async Task ExecuteAsync(SqlConnection connection, string sql, params SqlParameter[] parameters)
        {
            using (var cmd = new SqlCommand(sql, connection))
            {
                cmd.CommandTimeout = CommandTimeoutSeconds;
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
