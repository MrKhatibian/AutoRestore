using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    public partial class AutoRestore : Form
    {
        SqlConnection con;
        SqlCommand cmd;
        SqlDataReader dr;
        public AutoRestore()
        {
            InitializeComponent();
        }   
        
        private void frmAutoRestore_Load(object sender, EventArgs e)
        {
            label1.Visible = false;
            serverName(".");
        }

        //Copy .bak file from folder to backup sql folder
        private void Copy_Click(object sender, EventArgs e)
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
        
        //Make serverName Function for find ServerName and add to combo
        public void serverName(string str) 
        {            
            con = new SqlConnection("Data Source=.;Initial Catalog=Master;Integrated Security=SSPI;");
            con.Open();
            cmd = new SqlCommand("select * from sysservers where srvproduct='SQL Server'", con);
            dr = cmd.ExecuteReader();            
            MessageBox.Show(con.WorkstationId);
            //Find Server Name from data reader
            while (dr.Read())
            {
                comboBox1.Items.Add(dr[2]);
            }
            dr.Close();
        }
        //Make Connection to SQL server and find all Database for add to combo
        public void Createconnection()
        {
            con = new SqlConnection("Data Source=" + (comboBox2.Text) + ";Initial Catalog= Master;Integrated Security=SSPI;");
            con.Open();
            comboBox2.Items.Clear();
            cmd = new SqlCommand("select * from sysdatabases", con);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                comboBox2.Items.Add(dr[0]);
            }
            dr.Close();
        }
        //Make query object for T_SQL query
        public void query(string que)
        {
            // ERROR: Not supported in C#: OnErrorStatement
            cmd = new SqlCommand(que, con);
            cmd.ExecuteNonQuery();
        }
        //Make blank function for restore Select Database
        public void blank(string str)
        {
            if (string.IsNullOrEmpty(comboBox1.Text) | string.IsNullOrEmpty(comboBox2.Text))
            {
                // label3.Visible = true;
                MessageBox.Show("Server Name & Database can not be Blank");
                return;
            }
            else
            {
                query("USE master ALTER DATABASE WebMarket SET SINGLE_USER WITH ROLLBACK IMMEDIATE USE master ALTER DATABASE WebMarket SET MULTI_USER USE master RESTORE DATABASE WebMarket FROM DISK = '" + str +"' WITH REPLACE");
                //query("IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + comboBox2.Text + "') DROP DATABASE " + comboBox2.Text + " RESTORE DATABASE " + comboBox2.Text + " FROM DISK = '" + str +"'");                
                label1.Visible = true;
                label1.Text = "Database Backup file has been restore successfully"+ str;                
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Createconnection();
        }

        private void Restore_Click(object sender, EventArgs e)
        {
            label1.Text.Equals(null);
            //Get .bak file in dirctory 
            string[] filePaths = Directory.GetFiles(@"C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\Backup", "WebMarket*.bak", SearchOption.AllDirectories);
            //Find the last backup fil in directory
            string SourceDate = DateTime.Today.ToShortDateString();            
            string date = DateTime.ParseExact(SourceDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy_dd_MM");            
            
            foreach (string item in filePaths)
            {
                int dateIndex = item.IndexOf(date,0);
                if (item.Contains(date))
                    blank(item);                
            }            
        }
    }
}
