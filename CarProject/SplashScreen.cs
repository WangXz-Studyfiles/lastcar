using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
////using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace CarProject
{
    public partial class SplashScreen : Form
    {
        SQL sqlhelp = new SQL();
        public SplashScreen()
        {
            InitializeComponent();
        }

        private void SplashScreen_Load(object sender, EventArgs e)
        {
            string str = "select companyname from CP_name";
            DataSet dsDataSet1 = sqlhelp.ExecuteDataSet(str);
            label3.Text = dsDataSet1.Tables[0].Rows[0]["companyname"].ToString();


            //label3.Text = "汽车公司";
            this.Show();
            Application.DoEvents(); // Finish Paint
            Cursor.Current = Cursors.WaitCursor;

            // Simulate some activity (e.g. connect to database, caching data, retrieving defaults)
            this.labelStatus.Text = "Step 1";
            this.labelStatus.Refresh();
            System.Threading.Thread.Sleep(1000);

            // Simulate some activity
            this.labelStatus.Text = "Step 2";
            this.labelStatus.Refresh();
            System.Threading.Thread.Sleep(1000);

            // Simulate some activity
            this.labelStatus.Text = "Step 3";
            this.labelStatus.Refresh();
            System.Threading.Thread.Sleep(1000);

            // Close Form
            this.Close();	
        }
    }
}
