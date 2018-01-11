using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZedGraph;

namespace CarProject
{
    public partial class H_curve : Form
    {
        string start_time;
        string end_time;
        SQL mysql = new SQL();
        public PointPairList list_V1;//电压1
        public PointPairList list_V2;//电压2
        public PointPairList list_Vb;//电压3
        public PointPairList list_Ie;//电流1  
        public PointPairList list_R;//电流1     
        public PointPairList list_P;//能量
        public GraphPane myPane;
        public double[] v1_pointdata;
        public double[] v2_pointdata;
        public double[] v3_pointdata;
        public double[] i1_pointdata;
        public double[] R_pointdata;
        public double[] p_pointdata;
        public string[] x1;
        public string[] x2;
        public string[] x3;
        public string[] x4;
        public string[] x5;
        public string[] x6;
        public string items;


        AutoSizeFormClass asc = new AutoSizeFormClass();        //1.声明自适应类实例
        public H_curve()
        {
            InitializeComponent();
        }

        private void H_curve_Load(object sender, EventArgs e)
        {
            asc.controllInitializeSize(this);//调用类的初始化方法，记录窗体和其控件的初始位置和大小
            gb_timeselct.Visible = false;
            groupBox2.Visible = false;
            list_V1 = new PointPairList();
            list_V2 = new PointPairList();
            list_Vb = new PointPairList();
            list_Ie = new PointPairList();
            list_R = new PointPairList();  
            list_P = new PointPairList();
            button1.Visible = false;
            panel1.Visible = false;
        }
       //
       //时间查询
       //
       
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            dateTimePicker1.Enabled = true;
            dateTimePicker2.Enabled = true;
            maskedTextBox1.Enabled = true;
            maskedTextBox2.Enabled = true;
            start_time = Convert.ToDateTime(dateTimePicker1.Text).ToString("yyyy-MM-dd ") + maskedTextBox1.Text;
            end_time = Convert.ToDateTime(dateTimePicker2.Text).ToString("yyyy-MM-dd ") + maskedTextBox2.Text;
            gb_timeselct.Visible = true;
            groupBox2.Visible = false;
            button1.Visible = true;
            panel1.Visible = false;
        }
        public bool Istime(string str)
        {

            return System.Text.RegularExpressions.Regex.IsMatch(str, @"^((([01][0-9])|([2][0-3])):[0-5][0-9]:[0-5][0-9])$");
        }
        //
        //测试人查询
        //
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
           
            string sql  = "select distinct people from data";//查询，查找。
            DataSet ds = mysql.ExecuteDataSet(sql);//将查询的数据返回
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    listBox2.Items.Add(ds.Tables[0].Rows[i]["people"].ToString());//添加到listbox
                }
            }
            else
            {
                MessageBox.Show("文件记录为空！");
            }
            groupBox2.Text = "请选择测试人";
            groupBox2.Visible = true;
            gb_timeselct.Visible = false;
            button1.Visible = false;
            panel1.Visible = false;
        }
        //
        //测试车型查询
        //
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            string sql = "select distinct car_type from data";
            DataSet ds = mysql.ExecuteDataSet(sql);
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    listBox2.Items.Add(ds.Tables[0].Rows[i]["car_type"].ToString());
                }
            }
            else
            {
                MessageBox.Show("文件记录为空！");
            }
            groupBox2.Text = "请选择测试车型";
            groupBox2.Visible = true;
            gb_timeselct.Visible = false;
            button1.Visible = false;
            panel1.Visible = false;
        }

        //
        //测试公司
        //
        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            string sql = "select distinct company from data";
            DataSet ds = mysql.ExecuteDataSet(sql);
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    listBox2.Items.Add(ds.Tables[0].Rows[i]["company"].ToString());
                }
            }
            else
            {
                MessageBox.Show("文件记录为空！");
            }
            groupBox2.Text = "请选择测试公司";
            groupBox2.Visible = true;
            gb_timeselct.Visible = false;
            button1.Visible = false;
            panel1.Visible = false;
        }
       
        private void listBox2_DoubleClick(object sender, EventArgs e)
        {
            listBox1.Items.Clear();          
            //string items;
            string sql;
            DataSet ds;
            try
            {
                items = listBox2.SelectedItem.ToString();
            }
            catch 
            {
                DialogResult dr;
                dr = MessageBox.Show("未选中！！\n请确认点击到正确的信息！", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
               
            }
            //items = listBox2.SelectedItem.ToString();
          
            if (radioButton2.Checked == true)
            {
                sql = "select  datetime from data where people='" + items + "'";
                ds = mysql.ExecuteDataSet(sql);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        listBox1.Items.Add(ds.Tables[0].Rows[i]["datetime"].ToString());
                    }
                }
            }
            if (radioButton3.Checked == true)
            {
                sql = "select  datetime from data where car_type='" + items + "'";
                ds = mysql.ExecuteDataSet(sql);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        listBox1.Items.Add(ds.Tables[0].Rows[i]["datetime"].ToString());
                    }
                }
            }
            if (radioButton4.Checked == true)
            {
                sql = "select  datetime from data where company='" + items + "'";
                ds = mysql.ExecuteDataSet(sql);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        listBox1.Items.Add(ds.Tables[0].Rows[i]["datetime"].ToString());
                    }
                }
            }           
        }
        private double x_data_count(double i)
        {
            i = i + 0.005;

            return i;
        }
        //曲线显示
        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            double x = 0;
            //double xx = 0;
            //double xxx = 0;
            //double xxxx = 0;
            //double xxxxx = 0;
            list_R.Clear();
            list_V1.Clear();
            list_V2.Clear();
            list_Vb.Clear();
            list_Ie.Clear();
            list_P.Clear();
            radioButton5.Checked = false;
            radioButton6.Checked = false;
            radioButton7.Checked = false;
            radioButton8.Checked = false;
            checkBox1.Checked = false;
            checkBox2.Checked = false;
            checkBox3.Checked = false;
            checkBox4.Checked = false;
            checkBox5.Checked = false;
            checkBox6.Checked = false;
            this.zedGraphControl1.GraphPane.CurveList.Clear(); //当选中文件双击时，清除上次画的波形图
            zedGraphControl1.Refresh();
            try
            {
                items = listBox1.SelectedItem.ToString();
            }
            catch 
            {
                DialogResult dr;
                dr = MessageBox.Show("未选中！！\n请确认点击到正确的信息！", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
               
            }
            string sql = "select * from  data where datetime='" + items + "'";

            try
            {
                DataSet ds = mysql.ExecuteDataSet(sql);
           
            if (ds.Tables[0].Rows.Count > 0)
            {
                string str_v1 = ds.Tables[0].Rows[0]["vol_1"].ToString();//获取count行的电流    
                string str_v2 = ds.Tables[0].Rows[0]["vol_2"].ToString();//获取dataset第0张表，第一行，第vol_2列数据
                string str_v3 = ds.Tables[0].Rows[0]["vol_3"].ToString();
                string str_i1 = ds.Tables[0].Rows[0]["I_1"].ToString();
                string str_R2 = ds.Tables[0].Rows[0]["R"].ToString();
                string str_p  = ds.Tables[0].Rows[0]["Power"].ToString();
                string[] v1 = str_v1.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                string[] v2 = str_v2.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                string[] v3 = str_v3.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                string[] i1 = str_i1.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                string[] R = str_R2.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                string[] p  = str_p.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                v1_pointdata=new double[v1.Length ];
                v2_pointdata=new double[v2.Length ];
                v3_pointdata=new double[v3.Length ];
                i1_pointdata=new double[i1.Length ];
                R_pointdata = new double[R.Length];
                p_pointdata = new double[p.Length];
                //x1 = new string[v1.Length];
                //x2 = new string[v2.Length];
                //x3 = new string[v3.Length];
                //x4 = new string[i1.Length];
                //x5 = new string[R.Length];
                //x6 = new string[p.Length];
                for (int i = 0; i < v1.Length; i++)
                {
                    v1_pointdata[i] = Convert.ToDouble(v1[i].ToString());//V1
                    x = x_data_count(x);
                    list_V1.Add(x, v1_pointdata[i]);
                }
                x = 5;
                for (int i = 1000; i < v2.Length; i++)
                {
                    v2_pointdata[i] = Convert.ToDouble(v2[i].ToString());//V2
                    x = x_data_count(x);
                    list_V2.Add(x, v2_pointdata[i]);
                }
                x = 5;
                for (int i = 1000; i < v3.Length; i++)
                {
                    v3_pointdata[i] =Convert.ToDouble(v3[i].ToString());//Vb
                    x = x_data_count(x);
                    list_Vb.Add(x, v3_pointdata[i]);
                }
                x = 0;
                for (int i = 0; i < i1.Length; i++)
                {
                    i1_pointdata[i] =Convert.ToDouble(i1[i].ToString());//Ie
                    x = x_data_count(x);
                    list_Ie.Add(x, i1_pointdata[i]);
                }
                for (int i = 0; i < R.Length; i++)
                {
                    R_pointdata[i] = Convert.ToDouble(R[i].ToString());//R
                    list_R.Add(i, R_pointdata[i]);
                }
                x = 0;
                for (int i = 0; i < p.Length; i++)
                {
                    p_pointdata[i] =Convert.ToDouble(p[i].ToString());//E
                    x = x_data_count(x);
                    list_P.Add(x, p_pointdata[i]);
                }
                panel1.Visible = true;
                radioButton5.Checked = true;
                checkBox1.Checked = true;
                checkBox2.Checked = true;
                checkBox3.Checked = true;
               
            }
            }
            catch
            {

            }
        }
       
        //
        //电流
        //
        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton6.Checked == true)
            {
                checkBox1.Checked = false;
                checkBox1.Visible = false;
                checkBox2.Checked = false;
                checkBox2.Visible = false;
                checkBox3.Checked = false;
                checkBox3.Visible = false;
                checkBox5.Checked = false;
                checkBox5.Visible = false;
                checkBox6.Visible = false;
                checkBox6.Checked = false;
                checkBox4.Checked = true;
                checkBox4.Visible = true;
            }
        }
 
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //this.zedGraphControl1.RestoreScale(this.zedGraphControl1.GraphPane);  //恢复到原图
            //list_V1.Clear();
            creatGraph();
        }

        //
        //电压
        //
        private void radioButton5_CheckedChanged_1(object sender, EventArgs e)
        {
            if (radioButton5.Checked == true)
            {
                checkBox4.Checked = false;
                checkBox4.Visible = false;
                checkBox5.Checked = false;
                checkBox5.Visible = false;
                checkBox6.Visible = false;
                checkBox5.Checked = false;
                checkBox1.Visible = true;
                checkBox2.Visible = true;
                checkBox3.Visible = true;
                checkBox1.Checked = true;
                checkBox2.Checked = true;
                checkBox3.Checked = true;
            }
        }

        //
        //能量
        //
        private void radioButton7_CheckedChanged_1(object sender, EventArgs e)
        {
            if (radioButton7.Checked == true)
            {
                checkBox1.Checked = false;
                checkBox1.Visible = false;
                checkBox2.Checked = false;
                checkBox2.Visible = false;
                checkBox3.Checked = false;
                checkBox3.Visible = false;
                checkBox4.Checked = false;
                checkBox4.Visible = false;
                checkBox6.Visible = false;
                checkBox6.Checked = false;

                checkBox5.Checked = true;
                checkBox5.Visible = true;

            }
        }
        //鼠标悬停节点事件
        private string MyPointValueHandler(ZedGraphControl sender, GraphPane pane, CurveItem curve, int iPt)
        {

            PointPair pt = curve[iPt];
            if (curve.Label.Text == "Vb" || curve.Label.Text == "V1" || curve.Label.Text == "V2")
            {
                return "电压值" + pt.Y.ToString("#0.0000") + "\n时间:" + (pt.X).ToString("#0.000");

            }
            else if (curve.Label.Text == "Ie")
            {
                return "电流值" + pt.Y.ToString("#0.0000") + "\n时间:" + (pt.X).ToString("#0.000");
            }
            else if (curve.Label.Text == "Power")
            {
                return "能量值" + pt.Y.ToString("#0.0000") + "\n时间:" + (pt.X).ToString("#0.000");
            }
            else if (curve.Label.Text == "Res")
            {
                return "电阻值" + pt.Y.ToString("#0.0000") + "\n次数:" + (pt.X).ToString("#0");

            }
            else
            {
                return "";
            }
        }

        String[] szx;//X轴的日期数组
        public void creatGraph()
        {

            myPane = zedGraphControl1.GraphPane;
            this.zedGraphControl1.IsShowPointValues = true;//鼠标悬停事件，出现点坐标值
            zedGraphControl1.PointValueEvent += new ZedGraphControl.PointValueHandler(MyPointValueHandler);//手动添加鼠标悬停节点事件
            zedGraphControl1.GraphPane.XAxis.Scale.MaxAuto = true;
            zedGraphControl1.GraphPane.XAxis.Scale.MinAuto = true;
            zedGraphControl1.GraphPane.YAxis.Scale.MaxAuto = true;
            zedGraphControl1.GraphPane.YAxis.Scale.MinAuto = true;
            //zedGraphControl1.PanModifierKeys = Keys.None;           //直接可以用鼠标左键点击来拖拽
            zedGraphControl1.GraphPane.CurveList.Clear();
            if (checkBox6.Checked == true)
            {
                zedGraphControl1.GraphPane.XAxis.Title.Text = "测试次数";
            }
            else
            {
                zedGraphControl1.GraphPane.XAxis.Title.Text = "时间(s)";
            }
            zedGraphControl1.GraphPane.XAxis.Type = ZedGraph.AxisType.DateAsOrdinal;
            zedGraphControl1.GraphPane.XAxis.Scale.Min = 0;
            zedGraphControl1.GraphPane.XAxis.Scale.Max = 60;
            //zedGraphControl1.GraphPane.XAxis.Scale.MaxAuto = true;
            //zedGraphControl1.PanModifierKeys = Keys.None;//直接鼠标左键拖拽
            this.zedGraphControl1.GraphPane.XAxis.Type = ZedGraph.AxisType.Linear;          //可以显示到整数位后的小数位
            this.zedGraphControl1.ZoomStepFraction = 0.1;        //这是鼠标滚轮缩放的比例大小，值越大缩放就越灵敏 
            myPane.XAxis.Scale.MinorStep = 1;   //最小步长为0.1
            myPane.CurveList.Clear();


         
            if (radioButton5.Checked == true)
            {
                zedGraphControl1.GraphPane.Title.Text = "电压曲线";
                zedGraphControl1.GraphPane.YAxis.Title.Text = "电压[V]";
                if ((checkBox1.Checked == true) && (checkBox3.Checked == false) && (checkBox2.Checked == false))
                {
                    //list_V1.Clear();
                    //creat_list1(v1_pointdata, x1, list_V1);
                    
                    LineItem myCurve = myPane.AddCurve("Vb", list_V1, Color.Red, SymbolType.None);
                }
                if ((checkBox1.Checked == false) && (checkBox2.Checked == true) && (checkBox3.Checked == false))
                {
                    //list_V2.Clear();
                    //creat_list1(v2_pointdata, x2, list_V2);
                   
            
                    LineItem myCurve = myPane.AddCurve("V1", list_V2, Color.Green, SymbolType.None);
                }
                if ((checkBox1.Checked == false) && (checkBox2.Checked == false) && (checkBox3.Checked == true))
                {
                    //list_Vb.Clear();
                    //creat_list1(v3_pointdata, x3, list_Vb);
                    LineItem myCurve = myPane.AddCurve("V2", list_Vb, Color.Blue, SymbolType.None);
                }
                if ((checkBox1.Checked == true) && (checkBox2.Checked == true) && (checkBox3.Checked == false))
                {
                    //list_V1.Clear();
                    //list_V2.Clear();
                    //creat_list1(v1_pointdata, x1, list_V1);
                    //creat_list1(v2_pointdata, x2, list_V2);
                    LineItem myCurve = myPane.AddCurve("Vb", list_V1, Color.Red, SymbolType.None);
                    LineItem myCurve1 = myPane.AddCurve("V1", list_V2, Color.Green, SymbolType.None);
                }
                if ((checkBox1.Checked == true) && (checkBox3.Checked == true) && (checkBox2.Checked == false))
                {
                    //list_V1.Clear();
                    //list_Vb.Clear();
                    //creat_list1(v1_pointdata, x1, list_V1);
                    //creat_list1(v3_pointdata, x3, list_Vb);
                    LineItem myCurve = myPane.AddCurve("vb", list_V1, Color.Red, SymbolType.None);
                    LineItem myCurve1 = myPane.AddCurve("V2", list_Vb, Color.Blue, SymbolType.None);
                }
                if ((checkBox2.Checked == true) && (checkBox3.Checked == true) && (checkBox1.Checked == false))
                {
                    //list_V2.Clear();
                    //list_Vb.Clear();
                    //creat_list1(v2_pointdata, x2, list_V2);
                    //creat_list1(v3_pointdata, x3, list_Vb);
                    LineItem myCurve = myPane.AddCurve("V1", list_V2, Color.Green, SymbolType.None);
                    LineItem myCurve1 = myPane.AddCurve("V2", list_Vb, Color.Blue, SymbolType.None);
                }
                if ((checkBox1.Checked == true) && (checkBox3.Checked == true) && (checkBox2.Checked == true))
                {
                    //list_V1.Clear();
                    //list_V2.Clear();
                    //list_Vb.Clear();
                    //creat_list1(v1_pointdata, x1, list_V1);
                    //creat_list1(v2_pointdata, x2, list_V2);
                    //creat_list1(v3_pointdata, x3, list_Vb);
                    LineItem myCurve1 = myPane.AddCurve("Vb", list_V1, Color.Red, SymbolType.None);
                    LineItem myCurve2 = myPane.AddCurve("V1", list_V2, Color.Green, SymbolType.None);
                    LineItem myCurve3 = myPane.AddCurve("V2", list_Vb, Color.Blue, SymbolType.None);
                }

            }
            if (radioButton6.Checked == true)
            {
                zedGraphControl1.GraphPane.Title.Text = "电流曲线";
                zedGraphControl1.GraphPane.YAxis.Title.Text = "电流[A]";
               
                if (checkBox4.Checked == true)
                {

                    //list_Ie.Clear();
                    //creat_list1(i1_pointdata, x4, list_Ie);
                   
                    LineItem myCurve3 = myPane.AddCurve("Ie电流", list_Ie, Color.Blue, SymbolType.None);
                  
                }
            }
            if (radioButton7.Checked == true)
            {
                zedGraphControl1.GraphPane.Title.Text = "能量曲线";
                zedGraphControl1.GraphPane.YAxis.Title.Text = "能量[J]";
                if (checkBox5.Checked == true)
                {
                    //list_P.Clear();
                    //creat_list1(p_pointdata, x6, list_P);           
                    LineItem myCurve3 = myPane.AddCurve("Power", list_P, Color.Red, SymbolType.None);
                }
            }
            if (radioButton8.Checked == true)
            {
                zedGraphControl1.GraphPane.Title.Text = "绝缘电阻曲线";
                zedGraphControl1.GraphPane.YAxis.Title.Text = "电阻[Ω/V]";
                if (checkBox6.Checked == true)
                {
                    //list_R.Clear();
                    //creat_list1(R_pointdata, x5, list_R);
                    LineItem myCurve3 = myPane.AddCurve("Res", list_R, Color.Fuchsia, SymbolType.None);
                }
            }

            myPane.XAxis.Scale.TextLabels = szx;//添加日期到X轴
            zedGraphControl1.AxisChange();
            zedGraphControl1.Refresh();
            zedGraphControl1.Invalidate();
            

        }
        //private void creat_list1(double[] point,string[] x,PointPairList list)
        //{
        //    if (point != null && x!= null)
        //    {
        //        szx = new String[x.Length];
        //        for (int i = 0; i < x.Length - 1; i++)
        //        {
        //            szx[i] = x[i].ToString();
        //            list.Add((Convert.ToDouble(x[i])), point[i]);
        //        }
        //    }     
        //}
       
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            //this.zedGraphControl1.RestoreScale(this.zedGraphControl1.GraphPane);  //恢复到原图
            //list_V2.Clear();
            creatGraph();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            //this.zedGraphControl1.RestoreScale(this.zedGraphControl1.GraphPane);  //恢复到原图
            //list_Vb.Clear();
            creatGraph();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            //this.zedGraphControl1.RestoreScale(this.zedGraphControl1.GraphPane);  //恢复到原图
            //list_Ie.Clear();
            creatGraph();
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            //this.zedGraphControl1.RestoreScale(this.zedGraphControl1.GraphPane);  //恢复到原图
          
            creatGraph();
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            //this.zedGraphControl1.RestoreScale(this.zedGraphControl1.GraphPane);  //恢复到原图
            //list_P.Clear();
            creatGraph();
        }

      
        private void H_curve_SizeChanged(object sender, EventArgs e)
        {
            asc.controlAutoSize(this);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string date = maskedTextBox1.Text;
            string date1 = maskedTextBox2.Text;
            if (!Istime(date))
            {
                MessageBox.Show("请输入正确时间");
                return;
            }
            if (!Istime(date1))
            {
                MessageBox.Show("请输入正确时间");
                return;
            }
            string str_request = "select *  from data where  datetime>'" + Convert.ToDateTime(dateTimePicker1.Text).ToString("yyyy-MM-dd ") + " " + maskedTextBox1.Text + "' and datetime<'" + Convert.ToDateTime(dateTimePicker2.Text).ToString("yyyy-MM-dd ") + " " + maskedTextBox2.Text + "'  order by datetime asc";
            DataSet ds = mysql.ExecuteDataSet(str_request);

            listBox1.Items.Clear();
            listBox2.Items.Clear();
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    listBox1.Items.Add(ds.Tables[0].Rows[i]["datetime"].ToString());
                }
            }
            else
            {
                MessageBox.Show("文件记录为空！");
            }

        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton8.Checked == true)
            {
                checkBox1.Checked = false;
                checkBox1.Visible = false;
                checkBox2.Checked = false;
                checkBox2.Visible = false;
                checkBox3.Checked = false;
                checkBox3.Visible = false;
                checkBox4.Checked = false;
                checkBox4.Visible = false;
                checkBox5.Checked = false;
                checkBox5.Visible = false;
                checkBox6.Visible = true;
                checkBox6.Checked = true;
                zedGraphControl1.GraphPane.XAxis.Title.Text = "测试次数";
            }
        }

        private void checkBox6_CheckedChanged_1(object sender, EventArgs e)
        {
            //this.zedGraphControl1.RestoreScale(this.zedGraphControl1.GraphPane);  //恢复到原图
            //list_R.Clear();
            creatGraph();
        }

        private void zedGraphControl1_Load(object sender, EventArgs e)
        {
            //this.zedGraphControl1.RestoreScale(this.zedGraphControl1.GraphPane);  //恢复到原图
            //creatGraph();
        }

        private void zedGraphControl1_DoubleClick(object sender, EventArgs e)
        {
            this.zedGraphControl1.RestoreScale(this.zedGraphControl1.GraphPane);  //恢复到原图
            creatGraph();
        }

        

    }
}
