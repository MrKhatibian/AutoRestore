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
        }

         //Find all backup file in path by Database Name
        public List<string> findbackupfile(string path)
        {
            try
            {
                string[] allbakupfileFindet = Directory.GetFiles(path, databaseName + "*.bak", SearchOption.AllDirectories);
                return bakfileName(allbakupfileFindet);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
                throw;
            }            
        }

         //Find the last backup file in path
        public string lastbakfileFindet(List<string> allbackFile)
        {
            if (allbackFile.Count > 0)
            {
                List<int> hour = new List<int>();
                foreach (string bakFile in allbackFile)
                {
                    hour.Add(Convert.ToInt32(getBetween(bakFile, todayDate + "_", "_")));
                }
                string max = hour.Max().ToString();
                foreach (string findetlastbakfile in allbackFile)
                {
                    if (findetlastbakfile.Contains(max))
                        return findetlastbakfile;
                }
            }
            return null;
        }

        //Find string in between strings
        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                int Start, End;
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            return "";
        }

        //Make blank function for restore Select Database
        public void RestoreDB(string str)
        {
            
            sqlConnection = new SqlConnection("Data Source=.;Initial Catalog=Master;Integrated Security=SSPI;");
            try
            {
                //Make Connection to SQL Server                
                sqlConnection.Open();
                lblStatusInfo.Text = "Connection to SQL Server ";                
                TSQL_query("USE master ALTER DATABASE " + databaseName + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE USE master ALTER DATABASE " + databaseName + " SET MULTI_USER USE master RESTORE DATABASE " + databaseName + " FROM DISK = '" + str + "' WITH REPLACE");
                lblStatusInfo.Text = "DB " + databaseName + " Restore successfully with" + str;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }            
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
