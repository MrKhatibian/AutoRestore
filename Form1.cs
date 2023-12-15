using System;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    public partial class AutoRestore : Form
    {
        //Make Paramter
        #region
        SqlConnection sqlConnection;
        SqlCommand sqlCommand;
        string databaseName = "WebMarket";
        string sourcefolderPath = @"F:\OHM\Projeh\AutoMirorring\SQLBackup\";
        string destinationfolderPath = @"C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\Backup\";
        //Convert today date format to yyyy_MM_dd for find the last backup file
        string todayDate = DateTime.ParseExact(DateTime.Today.ToShortDateString(), "MM/dd/yyyy", CultureInfo.InvariantCulture).ToString("yyyy_MM_dd");
        #endregion
        public AutoRestore()
        {
            InitializeComponent();
        }   
        
        private void frmAutoRestore_Load(object sender, EventArgs e)
        {
            //Make List for bakup file in directories
            List<string> findetbakfileInDestination = findbackupfile(destinationfolderPath);
            List<string> findetbakfileInSource = findbackupfile(sourcefolderPath);
            List<string> todaybackupSourceName = new List<string>(); 
             //Chech for Sourece Connection is Ok
            if (findetbakfileInSource.Count != 0)
            {
                //Chech for Source file has update backup
                foreach (string sourcebakfile in findetbakfileInSource)
                {
                    if (sourcebakfile.Contains(todayDate))
                        todaybackupSourceName.Add(sourcebakfile);
                }
                //Ready destination folder for Restor Database
                if (todaybackupSourceName.Count != 0 )
                {                    
                    string lastbackupSourceName = lastbakfileFindet(todaybackupSourceName);
                    while (findbackupfile(destinationfolderPath).Count != 0 )
                    {
                        foreach (string oldbakfileIndestination in findetbakfileInDestination)
                        {
                            try
                            {
                                if (oldbakfileIndestination != lastbackupSourceName)
                                    File.Delete(destinationfolderPath + oldbakfileIndestination);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                                throw;
                            }
                        }
                        break;
                    }
                    if (lastbakfileFindet(findbackupfile(destinationfolderPath)) == null)
                    {
                        try
                        {
                            File.Copy(sourcefolderPath + lastbackupSourceName, destinationfolderPath + lastbackupSourceName);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            throw;
                        }
                    }
                    string lastbackupDetinationName = lastbakfileFindet(findbackupfile(destinationfolderPath));
                    RestoreDB(destinationfolderPath + lastbackupDetinationName);
                }
                else
                    lblStatusInfo.Text = "Isn't Update backup file in Source Path";
            }    
            else
                lblStatusInfo.Text = "Not bakup File in Source Path";
            //this.Close();





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
                {
                    try
                    {
                        File.Copy(SourcebakupPath, destinationFile + getlastbackupfullName(SourcebakupPath, databaseName), true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        throw;
                    }
                }                    
                    
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
