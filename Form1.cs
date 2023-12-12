using System;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    public partial class AutoRestore : Form
    {
        //Make Paramter in My Project
        SqlConnection con;
        SqlCommand cmd;
        string databaseName = "WebMarket";
        string sourceFile = @"F:\OHM\Projeh\AutoMirorring\SQLBackup";
        string destinationFile = @"C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\Backup\";

        public AutoRestore()
        {
            InitializeComponent();
        }   
        
        private void frmAutoRestore_Load(object sender, EventArgs e)
        {
            //Make Connection to SQL Server
            con = new SqlConnection("Data Source=.;Initial Catalog=Master;Integrated Security=SSPI;");
            con.Open();
            lblStatusInfo.Text = "Connection to SQL Server ";

            //Find the last backup fil in directory
            #region                        
            //Copy The Last Backup from Main Server To Mirror Backupfile
            string SourceDate = DateTime.Today.ToShortDateString();
            string date = DateTime.ParseExact(SourceDate, "MM/dd/yyyy", CultureInfo.InvariantCulture).ToString("yyyy_MM_dd");
            string[] sourcefilePaths = Directory.GetFiles(sourceFile, databaseName + "*.bak", SearchOption.AllDirectories);
            foreach (string SourcebakupPath in sourcefilePaths)
            {
                if (SourcebakupPath.Contains(date))
                    File.Copy(SourcebakupPath, destinationFile + getlastbackupfullName(SourcebakupPath, databaseName), true);
            }
            //Get .bak file in dirctory and Delete Old Backup file 
            string[] filePaths = Directory.GetFiles(destinationFile, databaseName + "*.bak", SearchOption.AllDirectories);
            foreach (string item in filePaths)
            {
                if (item.Contains(date))
                    blank(item);
                else
                    File.Delete(item);
            }
            #endregion

            //Find Server Name from data reader
            #region           
            //cmd = new SqlCommand("select * from sysservers where srvproduct='SQL Server'", con);
            //dr = cmd.ExecuteReader();
            //while (dr.Read())
            //{
            //    //dr[2] return server name
            //    comboBox1.Items.Add(dr[2]);
            //}
            //dr.Close();
            #endregion

            //Find DataBase in Server
            #region
            //cmd = new SqlCommand("select * from sysdatabases", con);
            //dr = cmd.ExecuteReader();
            //while (dr.Read())
            //{
            //    mydataBases[i] = dr[0].ToString();
            //    i++;
            //}
            //dr.Close();
            //lblStatusInfo.Text = "Find Databases";
            #endregion

            //this.Close();
        }

        //Make blank function for restore Select Database
        public void blank(string str)
        {
            query("USE master ALTER DATABASE "+ databaseName +" SET SINGLE_USER WITH ROLLBACK IMMEDIATE USE master ALTER DATABASE "+ databaseName +" SET MULTI_USER USE master RESTORE DATABASE "+ databaseName +" FROM DISK = '" + str +"' WITH REPLACE");                               
            lblStatusInfo.Text = "Database Backup file has been restore successfully with"+ str;           
        }

        //Make query object for T_SQL query
        public void query(string que)
        {
            // ERROR: Not supported in C#: OnErrorStatement
            cmd = new SqlCommand(que, con);
            cmd.CommandTimeout = 600;
            cmd.ExecuteNonQuery();
        }

        //Copy .bak file from Server to backup sql folder
        public void Copy(object sender, EventArgs e)
        {
            string sourceFile = @"F:\OHM\Projeh\AutoMirorring\SQLBackup\WebMarket.bak";
            string destinationFile = @"C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\Backup\WebMarket.bak";
            try
            {
                File.Copy(sourceFile, destinationFile, true);
                MessageBox.Show("Its Done");
            }
            catch (IOException iox)
            {
                Console.WriteLine(iox);
                throw;
            }
        }

                

        //Find String in SourcePath
        public static string getlastbackupfullName(string strSource, string strStart)
        {
            if (strSource.Contains(strStart))
            {
                int Start = strSource.IndexOf(strStart, 0);
                return strSource.Substring(Start);
            }
            return "";
        }
    }
}
