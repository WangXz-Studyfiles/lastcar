using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Runtime.InteropServices;

namespace CarProject
{
    public partial class login : Form
    {
        public string test_name;
        public string test_car_type;
        public string test_quality;
        public string test_people;
      
        public login()
        {
            InitializeComponent();
        }

        private void btn_login_Click(object sender, EventArgs e)
        {

             test_name=textBox1 .Text ;
             test_car_type = textBox2.Text;
             test_quality = textBox3.Text;
             test_people = textBox4.Text;
             this.Close();
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void login_Load(object sender, EventArgs e)
        {

        }

      
    }
}
