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
    public partial class Form1 : Form
    {
        SqlConnection con;
        SqlCommand cmd;
        SqlDataReader dr;
        public Form1()
        {
            InitializeComponent();
        }      
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

        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Visible = false;
            serverName(".");
        }
        public void serverName(string str) 
        {
            //con = new SqlConnection("DataSource=" + str + ";InitialCatalog= Master; uid=sa; pwd=Micr0s0ft;");
            con = new SqlConnection("Data Source=.;Initial Catalog= Master;Integrated Security=SSPI;");
            con.Open();
            cmd = new SqlCommand("select *  from sysservers  where srvproduct='SQL Server'", con);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                comboBox1.Items.Add(dr[2]);
            }
            dr.Close();
        }
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
        public void query(string que)
        {
            // ERROR: Not supported in C#: OnErrorStatement
            cmd = new SqlCommand(que, con);
            cmd.ExecuteNonQuery();
        }
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
                    //openFileDialog1.ShowDialog();
                    // string a = ComboBoxDatabaseName.Text.ToString();
                    query("IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + comboBox2.Text + "') DROP DATABASE " + comboBox2.Text + " RESTORE DATABASE " + comboBox2.Text + " FROM DISK = '" + str +"'");
                //query("IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + comboBox2.Text + "') DROP DATABASE " + comboBox2.Text + " RESTORE DATABASE " + comboBox2.Text + " FROM DISK = '" + openFileDialog1.FileName + "'");
                label1.Visible = true;
                    label1.Text = "Database Backup file has been restore successfully";                
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Createconnection();
        }

        private void Restore_Click(object sender, EventArgs e)
        {
            label1.Text.Equals(null);
            string[] filePaths = Directory.GetFiles(@"C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\Backup", "WebMarket*.bak", SearchOption.AllDirectories);
            //
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
