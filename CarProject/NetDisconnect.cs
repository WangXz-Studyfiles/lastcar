using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CarProject
{
    public partial class NetDisconnect : Form
    {
        public int Stop_flag;
        public NetDisconnect()
        {
            InitializeComponent();
        }

        private void NetDisconnect_Load(object sender, EventArgs e)
        {
         
        }
        private void NetDisconnect_DoubleClick(object sender, EventArgs e)
        {
            Stop_flag = 1;//双击后将标志位置1，在Form.cs程序中判断，为1则退出应用
            this.Close();
        }
    }
}
