using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZedGraph;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Diagnostics;//测试
//load以后，check事件发生，add.curve(Vb,V1,V2)。//Vb即程序代码中的V1，V1即代码中的V2，V2即代码中的V3
# region 按钮或文本name

//radiobutton1为电压，radiobutton2为电流
//checkbox123为Vb、V1、V2，checkbox6为Ie
//textbox1显示时间与绝缘电阻的次数
//textbox2显示绝缘电阻的阻值
 
//button1.button2.button3分别为能量测试、电阻测试和重新测试
//button4为退出测试、button5为查询历史曲线
//combobox1、combobox2分别为电阻的阻值与测量模式、textbox3为工作电压 


//timer1定时扫描联网、time2定时发送FF
# endregion
namespace CarProject
{
    public partial class Form1 : Form
    {
        public int i=0;//功率数组的第i个值
        public int ii=0;//与手机端同步显示功率的标志位;
        SQL mysql = new SQL();//类的实例化
        AutoSizeFormClass asc = new AutoSizeFormClass();        //1.声明自适应类实例
       
        /*SocktClient*/

        Socket socketClient = null;//创建 1个客户端套接字 和1个负责监听服务端请求的线程  
        Thread threadClient = null;//TCP接收线程
        IPEndPoint endpoint = null;//IP与端口号
        int port = 8899;//指定端口       
        //string IP = "10.10.100.254";
        string IP = "192.168.10.254";
        //电脑的网络共享中心--更改适配器--无线网络连接--属性--Internet协议版本4-属性--使用下面的IP地址-在IP地址列输入“192.168.10.3”，点击子网掩码自动生成地址。完成操作，连接无线网络。
        List<string> V1_array = new List<string>();//字符型电压数据
        List<string> V2_array = new List<string>();//字符型电压数据
        List<string> V3_array = new List<string>();//字符型电压数据
        List<string> I1_array = new List<string>();//字符型电流数据
        List<string> P_array = new List<string>(); //字符型功率数据
        List<string> R_array = new List<string>();//字符型电阻数据

        List<double> V1_point = new List<double>();//浮点型电压数据
        List<double> V2_point = new List<double>();//浮点型电压数据
        List<double> V3_point = new List<double>();//浮点型电压数据
        List<double> I1_point = new List<double>();  //浮点型电流数据    
        List<double> P_point = new List<double>();//浮点型功率数据
        List<double> R_point = new List<double>();//浮点型电阻数据

        List<byte>       data = new List<byte>();//4000字节数据
        List<byte>   data_Res = new List<byte>();//4字节绝缘电阻值数据
       
        public PointPairList list_V1    = new PointPairList();//电压Vb曲线
        public PointPairList list_V2    = new PointPairList();//电压V1曲线
        public PointPairList list_V3    = new PointPairList();//电压V2曲线
        public PointPairList list_I     = new PointPairList();//电流Ie曲线   
        public PointPairList list_P     = new PointPairList();//能量P曲线
        public PointPairList list_dianzu= new PointPairList();//电阻Res曲线
        public GraphPane myPane;    
        byte[] arrRecMsg = new byte[1024*4];//定义一个内存缓冲区 用于临时性存储接收到的信息;
        string strRecMsg;//socket接收转换字符     
        string[] chars    = new string[1000];
         float[] recedata = new float[1000];//将每秒4000个字节型数转成1000个浮点数
        string V1=null;
        string V2=null;
        string V3=null;
        string I1=null;       
        string power=null;
        int bytes = 0;//接收的字节数
        public int ResChoose=0;//电阻值选择改变的标志位
        public int VbChoose = 0;
        public float Float_Vb;
        //public int time_count=1;
        public int count =1;//收到4000指令，开始自加、、//原来默认为6，现在改为默认为1；
        public int res_x = 0;//电阻坐标计数
        public string title = "曲线";
        double x = 0;//描点曲线的横坐标，从0开始；

        byte [] VandRes=new byte[10];//测电阻的输入电压和模式，还有电阻值；
        
        //VandRes[8] = 0;
        float res;//电阻的浮点型
        public string ResValue;//选择的电阻值
        public string Vwork = "";//输入的工作电压
        public string Resmode;
        public string TestRes;

        public int time = 0;//开始的1-5秒显示

        NetDisconnect WifiNotlink = new NetDisconnect();//网络未连接窗体

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            VandRes[8] = 0x01;
            asc.controllInitializeSize(this);//调用类的初始化方法，记录窗体和其控件的初始位置和大小            
            radioButton1.Checked = true;
            checkBox1.Checked = true;
            checkBox2.Checked = true;
            checkBox3.Checked = true;
            energy.Visible = false;
            energy.Checked = false;
            RES_radio.Visible = false;
            RES_radio.Checked = false;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false ;
            //socket_set();          
            timer1.Start();

            label3.Visible = false;


            comboBox2.Text = "双表";
            comboBox1.Visible = false;
            comboBox2.Visible = false;
            label1.Visible = false;
            label2.Visible = false;
            label3.Visible = false;
            label4.Visible = false;
            label5.Visible = false;
            label8.Visible = false;
            textBox3.Visible = false;
            textBox2.Visible = false;
            


           
        }

        //鼠标悬停节点事件
        private string MyPointValueHandler(ZedGraphControl sender, GraphPane pane, CurveItem curve, int iPt)
        {

            PointPair pt = curve[iPt];
            if (curve.Label.Text == "Vb" || curve.Label.Text == "V1" || curve.Label.Text == "V2")
            {
                return "电压值" + pt.Y.ToString("#0.0000")+"\n时间:" + (pt.X).ToString("#0.000");

            }
            else if (curve.Label.Text == "Ie")
            {
                return "电流值" + pt.Y.ToString("#0.0000")+  "\n时间:" + (pt.X).ToString("#0.000");
            }
            else if (curve.Label.Text == "Power")
            {
                return "能量值" + pt.Y.ToString("#0.0000") + "\n时间:" + (pt.X).ToString("#0.000");
            }
            else if (curve.Label.Text == "Res")
            {
                return "电阻值" + pt.Y.ToString("#0.0000") + "\n时间:" + (pt.X).ToString("#0.000");

            }
            else
            {
                return "";
            }
        }

        public static bool Delay(int delayTime)
        {
            DateTime now = DateTime.Now;
            int s;
            do
            {
                TimeSpan spand = DateTime.Now - now;
                s = spand.Seconds;
                Application.DoEvents();
            }
            while (s < delayTime);
            return true;
        }
        public void socket_set()//TCP通信设置，实时判断连接！！！
        {
         
            try
            {
               
                socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //定义一个套字节监听  包含3个参数(IP4寻址协议,流式连接,TCP协议)
                endpoint = new IPEndPoint(IPAddress.Parse(IP), port); //将获取的ip地址和端口号绑定到网络节点endpoint上               
                //socketClient.Connect(endpoint);//这里客户端套接字连接到网络节点(服务端)用的方法是Connect 而不是Bind
                //socketClient.BeginConnect(IP, port, null, null);
                socketClient.BeginConnect(endpoint, null, null);


                //同步连接Connect，在没有网络的情况下，返回时间很长，大约10s
                //异步连接接收检测发现有不稳定的情况，但是异步连接可以判断网络是否处于连接状态，
                //因此判断网络是否连接，用异步连接判断，并连接，若没有连接，则继续扫描，若有连接则关闭连接，重新用同步连接。
                Delay(1);//为防止连接很慢，connected检测不出来。延时一秒
                if (socketClient.Connected)
                {
                    socketClient.Close();
                    //Delay(1);
                    socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //定义一个套字节监听  包含3个参数(IP4寻址协议,流式连接,TCP协议)
                    endpoint = new IPEndPoint(IPAddress.Parse(IP), port);
                    socketClient.Connect(endpoint);
                    //socketClient.Connect(endpoint);
                   
                   
                    label3.Text = "网络已连接";
                    label3.Visible = true;
                    
                    threadClient = new Thread(RecMsg);  //创建一个线程 用于监听服务端发来的消息
                    threadClient.IsBackground = true; //将窗体线程设置为与后台同步
                    threadClient.Start(); //启动线程    

                    //if (WifiNotlink.Visible == true)
                    
                    //{
                    //    WifiNotlink.Close();
                    //}
                    timer2.Start();//每秒发送FF,
                    timer1.Stop();
                }
                else if (socketClient.Connected==false)//若没有连接，则弹出窗体，并显示网络未连接
                {
                    label3.Text = "网络未连接";
                    label3.Visible = true;
                    //if (WifiNotlink.Visible == false)
                    //{
                    //    WifiNotlink.ShowDialog();
                    //}    
                }     
            }           
            catch
            { 
                label3.Text = "网络未连接";
            }
       
        }

        private void RecMsg()
        {
            try
            {
                while ((bytes = socketClient.Receive(arrRecMsg, arrRecMsg.Length, 0)) > 0)//这里不需要while（1）；
                {
                    if ((bytes == 5))
                    {
                        //检测发现有乱点的状况，分析有可能出现一次发送5个字节，
                        //若出现5字节，则会进去此判断语句，清掉此5个字节，导致后面接收的4000字节乱序。
                        //因此此5个字节必须是下面3个才进行判断，否则退出判断，进入4000字节中。
                        strRecMsg = Encoding.ASCII.GetString(arrRecMsg, 0, bytes);//将ASCLL码转化为字符串
                        if ((strRecMsg == "5OFFA") || (strRecMsg == "5ONNA") || (strRecMsg == "5ENDA"))//(("50FFA") | ("50NNA") | ("5ENDA")))
                        {
                            Array.Clear(arrRecMsg, 0, arrRecMsg.Length);
                            showstrs(strRecMsg);//按钮可显示选择
                            bytes = 0;
                        }

                    }
                    if ((bytes == 4) && (RES_radio.Checked = true))
                    {
                        //进去4个字节判断语句，必须是在电阻按钮出去按下的状态，才进入判断。
                        for (int j = 0; j < bytes; j++)
                        {
                            data_Res.Add(arrRecMsg[j]);     //添加电阻值                      
                        }

                        deal_byte_data();//处理电阻值，显示在文本框，并将值Add到曲线上，描点画图

                    }
                    else
                    {
                        for (int j = 0; j < bytes; j++)
                        {
                            data.Add(arrRecMsg[j]);//添加3电压，1电流，1能量值
                        }
                        deal_byte_data();//处理值，将1组数据的五个值分别放入五个数组中
                        Array.Clear(arrRecMsg, 0, arrRecMsg.Length);
                        bytes = 0;
                    }
                }
            }
            catch
            {
                //MessageBox.Show("TCP未连接555！");
            }
             
            }

       
        private void deal_byte_data()//处理接收到的字节数据，4字节转换为电阻值，4000字节用encode解析为各自的电压值
        {
            if (data_Res.Count == 4)
            {
                res = endcode(data_Res, 0);
                //res = res / Float_Vb;
                float_res(res);
                for (int s = 0; s < 4; s++)//遍历list查询重复的数据
                {
                    data_Res.RemoveAt(0);//Remove at 0
                }
            }
            else
            {
                data_Res.Clear();
            }
            if (data.Count >=4000)
            {
                
                ByteToData(data);//数据处理，将四千字节转为1000个浮点数
                encodedata(5, 0, recedata, V1, V1_array, V1_point);
                encodedata(5, 1, recedata, V2, V2_array, V2_point);
                encodedata(5, 2, recedata, V3, V3_array, V3_point);
                encodedata(5, 3, recedata, I1, I1_array, I1_point);
                encodedata(5, 4, recedata, power, P_array, P_point);
                try//与手机端同步显示P
                {
                    float str=5.20f;
                    if (P_point[0] > 0)
                    {
                        
                        if (ii == 0)
                        {
                            float_p(str);
                            ii = 1;
                        }
                        
                    }
                }
                catch
                {
                    MessageBox.Show("判断错误");
 
                }
                float_mode(recedata);//将五组数据，以x轴每0.01增加，添加到list曲线上。               
               
            }

        }

        private delegate void floar_Pow(float invokefun);//收到电阻值自动切到电阻曲线，手机端与PC
        private void float_p(float str)
        {

            if (this.InvokeRequired)
            {
                floar_Pow invoke = new floar_Pow(float_p);
                this.Invoke(invoke, str);
            }
            else
            {
                button1.Enabled = false;
                energy.Checked = true;
                RES_radio.Checked = false;
                creatGraph();
            }

        }
        //浮点数转换浮点数组
        public void ByteToData(List<byte> arrMsgRec)//每四个数据转换为一个浮点型数据
        {
           
            //将接收的字节数组转为浮点
            for (int j = 0; j < 1000; j++)
            {
                recedata[j] = endcode(arrMsgRec, 4 * j); 
            }
            for (int s = 3999; s >= 0; s--)//遍历list查询重复的数据
            {
                data.RemoveAt(s);
            }
            
        }
        //单个浮点数转换
        public float endcode(List<byte>  data, int i)
        {
                float outdata;          
                byte[] by = new byte[4];
                for (int s = 0; s < by.Length; s++)
                {
                    by[s] = data[i++];                   
                }
               outdata = BitConverter.ToSingle(by, 0);                                          
            return outdata; 
        }       

        #region 2000字节描点
        //处理2000字节浮点数
        private delegate void bytesshow(float[] invokefun);
        private void float_mode(float[] str)
        {
          
            if (str.Length != 0)
            {
                if (this.InvokeRequired)
                {
                    bytesshow invoke = new bytesshow(float_mode);
                    this.Invoke(invoke, str);
                }
                else
                {
                    
                    label6.Text = (count++).ToString();//时间显示                 
                    for (int i = 0; i <200; i++)
                    {

                        if (count <= 6)//前5秒显示,只显示电压//
                        {
                            x = x_data_count(x);//描点的X轴值
                            if (x < 1)//经过测试发现几乎每次的前一秒都有值为0，（单片机bug）若有0，则将前一个点的值赋给值为0的那个点
                            {
                                if (V1_point[i] == 0)//由于前一秒只显示Vb的值，则只需要判断v1是否等于0
                                {
                                    if (i == 0)//后来测试发现，出现第一秒卡顿，经分析，发现有可能第0个点为0，则 V1_point[i] = V1_point[i - 1];出错，
                                    {
                                        //后来测试发现，出现第一秒卡顿，经分析，发现有可能第0个点为0，
                                        //则 V1_point[i] = V1_point[i - 1];出错，因此需要扫描一个不为0的点，用do-while循环。
                                        do
                                        {
                                            V1_point[0] = V1_point[i++];
                                        }
                                        while (V1_point[0] != 0);
                                    }

                                    V1_point[i] = V1_point[i - 1];//将后面的值赋给前面的值。

                                }
                                list_V1.Add(x, V1_point[i]);
                            }
                            else//第二秒直接添加。
                            {
                                list_V1.Add(x, V1_point[i]);
                            }
                        }
                        else//第6秒，开始全部显示//
                        {
                            x = x_data_count(x);//描点的X轴值
                            list_V1.Add(x, V1_point[i]);
                            list_V2.Add(x, V2_point[i]);
                            list_V3.Add(x, V3_point[i]);
                            list_I.Add(x, I1_point[i]);
                            list_P.Add(x, P_point[i]);
                        }
                    }
                    

                    Array.Clear(recedata, 0, recedata.Length);
                    Array.Clear(arrRecMsg, 0, arrRecMsg.Length);
                    V1_point.Clear();
                    V2_point.Clear();
                    V3_point.Clear();
                    I1_point.Clear();
                    P_point.Clear();
                    if (count < 60)//小于60s固定的横坐标
                    {
                        zedGraphControl1.GraphPane.XAxis.Scale.Max = 60;
                    }
                    else if (count >= 60)//大于60s，横坐标自动
                    {
                        zedGraphControl1.GraphPane.XAxis.Scale.MaxAuto = true;
                    }
                    zedGraphControl1.AxisChange();//更新x，y范围
                    zedGraphControl1.Refresh();//重新刷新 
                    zedGraphControl1.Invalidate();
               }
           }    
        }
        #endregion
      

        //处理单个浮点数，接收到电阻值后描点划线
        private delegate void floar_R(float invokefun);
        private void float_res(float str)
        {
                string strstr = ResValue;
                if (this.InvokeRequired)
                {
                    floar_R invoke = new floar_R(float_res);
                    this.Invoke(invoke, str);
                }
                else
                {
                    zedGraphControl1.GraphPane.CurveList.Clear();
                    Array.Clear(recedata, 0, recedata.Length);
                    Array.Clear(arrRecMsg, 0, arrRecMsg.Length);
                    panel1.Visible = false;
                    //time_count = 0;

                    button2.Text = "绝缘电阻测试";
                    button2.Enabled = true;

                    button3.Enabled = true;
                    //button1.Enabled = false;
                    //energy.Checked = false;
                    //RES_radio.Checked = true;
                    byte[] data = new byte[1024];
                    label7.Text = "测试次数：";
                    this.zedGraphControl1.IsShowPointValues = true;//鼠标悬停事件，出现点坐标值
                    GraphPane myPane = zedGraphControl1.GraphPane;
                    zedGraphControl1.PointValueEvent += new ZedGraphControl.PointValueHandler(MyPointValueHandler);//手动添加鼠标悬停节点事件

                    zedGraphControl1.GraphPane.XAxis.Scale.MaxAuto = true;
                    zedGraphControl1.GraphPane.XAxis.Scale.MinAuto = true;
                    zedGraphControl1.GraphPane.YAxis.Scale.MaxAuto = true;
                    zedGraphControl1.GraphPane.YAxis.Scale.MinAuto = true;
                    //zedGraphControl1.PanModifierKeys = Keys.None;           //直接可以用鼠标左键点击来拖拽
                    zedGraphControl1.GraphPane.XAxis.Title.Text = "测试次数";
                    zedGraphControl1.GraphPane.XAxis.Type = ZedGraph.AxisType.Linear;
                    zedGraphControl1.GraphPane.Title.Text = "绝缘电阻";
                    zedGraphControl1.GraphPane.YAxis.Title.Text = "绝缘电阻阻值(Ω/V)";
                    zedGraphControl1.GraphPane.XAxis.Scale.Min = 0;
                    zedGraphControl1.GraphPane.Y2Axis.IsVisible = false;   // 使Y2轴不可见  

                    LineItem myCurve = myPane.AddCurve("Res", list_dianzu, Color.DeepPink, SymbolType.None);

                    zedGraphControl1.AxisChange();
                    zedGraphControl1.Refresh();
                    zedGraphControl1.Invalidate();

                    res_x++;
                    label6.Text = Convert.ToString(res_x);
                    textBox2.Text =Convert.ToString (str);                   
                    R_array.Add(Convert.ToString (str));
                    list_dianzu.Add(res_x, Convert.ToDouble(str));
                    zedGraphControl1.AxisChange();//更新x，y范围
                    zedGraphControl1.Refresh();//重新刷新 
                    zedGraphControl1.Invalidate();
                }
    
        }

        //处理字符串事件
        private delegate void mm(string invokefun);
        private void showstrs(string recv_strs)
        {
                  
            if (recv_strs.Length != 0)
            {
                if (this.InvokeRequired)
                {
                    mm invoke = new mm(showstrs);
                    this.Invoke(invoke, recv_strs);
                }
                else
                {
                    if (recv_strs == "5OFFA")//能量模式
                        {
                            button1.Visible = true;
                            button1.Enabled = true;
                            button2.Visible = false;
                            button2.Enabled = false;//电阻按钮相关显示取消
                            
                            //label8.Visible = false;
                            //textBox2.Visible = false;
                        }
                     else if (recv_strs == "5ONNA")//电阻模式
                        {
                            button1.Visible = false;//能量按钮相关显示取消
                            button1.Enabled = false;
                            button2.Visible = true;
                            button2.Enabled = true;

                            //comboBox2.Text = "双表";
                            comboBox1.Visible = true;
                            comboBox2.Visible = true;
                            label1.Visible = true;
                            label2.Visible = true;
                            label3.Visible = true;
                            label4.Visible = true;
                            label5.Visible = true;
                            label8.Visible = true;
                            textBox3.Visible = true;
                            textBox2.Visible = true;
                            //label1.Visible = true;
                            //comboBox1.Visible = true;
                            //label8.Visible = true;
                            //textBox2.Visible = true;
                        }
                     else if (recv_strs == "5VB6A")
                     {
                         MessageBox.Show("VB电压值过低");
 
                     }
                     else if (recv_strs == "5ENDA")
                     {
                         button3.Enabled = true;
                         button1.Enabled = false;
                     }


                    }             
            }
        }

      /// <summary>
      /// 数据解析
      /// </summary>
      /// <param name="length1"></数据长度>
      /// <param name="data"></长度>
      /// <param name="locate"></点位置>
      /// <param name="bytes"></数组>
      /// <param name="str"></param>
      /// <param name="array"></param>
      /// <param name="arr"></param>
   private void encodedata(int data, int locate, float[] bytes, string str, List<string> array, List<double> arr)
  {
      float[] check_result = new float[200];
      for (int i = 0; i < 200; i++)
      {
          check_result[i] = (bytes[i * data + locate]);
      }
      for (int s = 0; s < 200; s++)
      {
          array.Add(Convert.ToString(check_result[s]));
          arr.Add(Convert.ToDouble(check_result[s]));
      }
     
          
   }
   private void button1_Click(object sender, EventArgs e)//测试能量
      {
          ii = 1;//与手机同步
          //button2.Enabled = false;
          button1.Enabled = false;
          //绝缘电阻lable与text不显示
          energy.Checked = true;
          //RES_radio.Checked = false;
          //后来加的，有时候点击能量测试，能量曲线有时候加不上曲线控件上，不知能否
          //this.zedGraphControl1.RestoreScale(this.zedGraphControl1.GraphPane);  //恢复到原图,      后来加的，有时候点击能量测试，能量曲线有时候加不上曲线控件上，不知能否           
          creatGraph();
          byte[] data = new byte[1024];
          string str = "5PowA";
          byte[] bytetest = System.Text.Encoding.Default.GetBytes(str.ToString());
          try
          {
              socketClient.Send(bytetest);
          }
          catch
          {
              DialogResult dr;
              dr = MessageBox.Show("网络连接不正常");
          }
        
      }
        //
        //重新测试
        //
   private void button3_Click(object sender, EventArgs e)
   {
       button3.Enabled = false;
       DialogResult dr;
       dr = MessageBox.Show("是否将上次测试记录存入数据库？", "信息提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
       if (dr == DialogResult.Yes)
       {
           insert_sql();//存数据库
       }
       i = 0;
       ii = 0;
       //time_count = 1;
       //count = 1;
       x = 0;
       time = 0;
       count = 1;
       res_x = 0;
       Array.Clear(arrRecMsg, 0, arrRecMsg.Length);
       Array.Clear(recedata, 0, recedata.Length);

       label7.Text = "测试时间：";
       label6.Text = "0";
       
       textBox2.Text = "";
       panel1.Visible = true;
       button1.Enabled = false;
       button1.Visible = true;
       button2.Enabled = false;
       button2.Visible = true;
       button2.Text = "绝缘电阻测试";
       energy.Checked = false;
       RES_radio.Checked = false;


       comboBox2.Text = "双表";
       comboBox1.Visible = false;
       comboBox2.Visible = false;
       label1.Visible = false;
       label2.Visible = false;
       //label3.Visible = false;
       label4.Visible = false;
       label5.Visible = false;
       label8.Visible = false;
       textBox3.Visible = false;
       textBox2.Visible = false;

       data.Clear();
       list_dianzu.Clear();
       data_Res.Clear();
       data.Clear();


       list_V1.Clear();
       list_V2.Clear();
       list_V3.Clear();
       list_I.Clear();
       list_P.Clear();

       V1_array.Clear();
       V2_array.Clear();
       V3_array.Clear();
       I1_array.Clear();
       P_array.Clear();
       R_array.Clear();
       ////
       //this.zedGraphControl1.RestoreScale(this.zedGraphControl1.GraphPane);  //恢复到原图
       //zedGraphControl1.GraphPane.CurveList.Clear();
       //
       creatGraph();
       string str = "5StaA";
       byte[] bytetest = System.Text.Encoding.Default.GetBytes(str.ToString());
       try
       {
           socketClient.Send(bytetest);
       }
       catch
       {
           DialogResult ds;
           ds = MessageBox.Show("网络连接不正常");
       }
   }
    

      private void checkBox1_CheckedChanged(object sender, EventArgs e)
      {
          //this.zedGraphControl1.RestoreScale(this.zedGraphControl1.GraphPane);  //恢复到原图
          creatGraph();

      }
     
      private void radioButton1_CheckedChanged(object sender, EventArgs e)
      {
         
          if (radioButton1.Checked == true)
          {
             
              checkBox6.Visible = false;
              checkBox1.Visible = true;
              checkBox2.Visible = true;
              checkBox3.Visible = true;
              checkBox1.Checked = true;
              checkBox2.Checked = true;
              checkBox3.Checked = true;          
          }        
      }

      private void radioButton2_CheckedChanged(object sender, EventArgs e)
      {
          if (radioButton2.Checked == true)
          {
              checkBox1.Checked = false;
              checkBox1.Visible = false;
              checkBox2.Checked = false;
              checkBox2.Visible = false;
              checkBox3.Checked = false;
              checkBox3.Visible = false;         
              checkBox6.Visible = true;
              checkBox6.Checked = true;       
          }      
      }
      private void checkBox2_CheckedChanged(object sender, EventArgs e)
      {
          //this.zedGraphControl1.RestoreScale(this.zedGraphControl1.GraphPane);  //恢复到原图
          creatGraph();
      }

      private void checkBox3_CheckedChanged(object sender, EventArgs e)
      {
          //this.zedGraphControl1.RestoreScale(this.zedGraphControl1.GraphPane);  //恢复到原图
          creatGraph();
      }

     
      public void creatGraph()//可以修改能量的显示时间
      {

          GraphPane myPane = zedGraphControl1.GraphPane;
          zedGraphControl1.GraphPane.CurveList.Clear();//dk
          
          myPane.XAxis.Title.Text = "时间(s)";

          if (radioButton1.Checked == true)
          {
              myPane.YAxis.Title.Text = "电压[V]";
              title = "电压曲线";
          }
          if (radioButton2.Checked == true)
          {
              myPane.YAxis.Title.Text = "电流[A]";
              title = "电流曲线";
          }
          if (energy.Checked == true)
          {
              zedGraphControl1.GraphPane.Y2Axis.Title.Text = "能量[J]";
              zedGraphControl1.GraphPane.Y2Axis.IsVisible = true;   // 使Y2轴不可见  
              if (radioButton1.Checked == true)
              {
                  title = "电压、能量曲线";
              }
              if (radioButton2.Checked == true)
              {
                  title = "电流、能量曲线";
              }

          }
          else
          {
              zedGraphControl1.GraphPane.Y2Axis.IsVisible = false;   // 使Y2轴不可见  
          }
          myPane.Title.Text = title;
          myPane.XAxis.Scale.Min = 0;
          myPane.YAxis.Scale.Min = 0;
          this.zedGraphControl1.IsShowPointValues = true;//鼠标悬停事件，出现点坐标值
          zedGraphControl1.PointValueEvent += new ZedGraphControl.PointValueHandler(MyPointValueHandler);//手动添加鼠标悬停节点事件
          myPane.XAxis.Type = AxisType.Text;
          zedGraphControl1.GraphPane.XAxis.Scale.MaxAuto = true;
          zedGraphControl1.GraphPane.XAxis.Scale.MinAuto = true;
          zedGraphControl1.GraphPane.YAxis.Scale.MaxAuto = true;
          zedGraphControl1.GraphPane.YAxis.Scale.MinAuto = true;
          //zedGraphControl1.PanModifierKeys = Keys.None;           //直接可以用鼠标左键点击来拖拽
          zedGraphControl1.GraphPane.Y2Axis.MajorTic.IsOpposite = false;//不在y轴显示y2轴的坐标
          zedGraphControl1.GraphPane.Y2Axis.MinorTic.IsOpposite = false;
          zedGraphControl1.GraphPane.Y2Axis.MajorTic.IsInside = false;
          zedGraphControl1.GraphPane.Y2Axis.MinorTic.IsInside = false;
          zedGraphControl1.GraphPane.YAxis.MajorGrid.IsZeroLine = false;//不显示Y轴0点线
          zedGraphControl1.GraphPane.XAxis.Scale.Max = 60;
          //if (count < 60)
          //{
          //  zedGraphControl1.GraphPane.XAxis.Scale.Max = 60;
          //}
          //else if (count >= 60)
          //{
          //    zedGraphControl1.GraphPane.YAxis.Scale.MaxAuto = true;
          //}
          
          zedGraphControl1.GraphPane.XAxis.Type = ZedGraph.AxisType.Linear;
          if (radioButton1.Checked == true) 
          {
             
              if ((checkBox1.Checked == true) && (checkBox3.Checked == false) && (checkBox2.Checked == false))
              {
                  LineItem myCurve = myPane.AddCurve("Vb", list_V1, Color.Red, SymbolType.None);
              }
              if ((checkBox1.Checked == false) && (checkBox2.Checked == true) && (checkBox3.Checked == false))
              {
                  LineItem myCurve = myPane.AddCurve("V1", list_V2, Color.Green, SymbolType.None);
              }
              if ((checkBox1.Checked == false) && (checkBox2.Checked == false) && (checkBox3.Checked == true))
              {
                  LineItem myCurve = myPane.AddCurve("V2", list_V3, Color.Blue, SymbolType.None);
              }
              if ((checkBox1.Checked == true) && (checkBox2.Checked == true) && (checkBox3.Checked == false))
              {
                  LineItem myCurve = myPane.AddCurve("Vb", list_V1, Color.Red, SymbolType.None);
                  LineItem myCurve1 = myPane.AddCurve("V1", list_V2, Color.Green, SymbolType.None);
              }
              if ((checkBox1.Checked == true) && (checkBox3.Checked == true) && (checkBox2.Checked == false))
              {
                  LineItem myCurve = myPane.AddCurve("Vb", list_V1, Color.Red, SymbolType.None);
                  LineItem myCurve1 = myPane.AddCurve("V2", list_V3, Color.Blue, SymbolType.None);
              }
              if ((checkBox2.Checked == true) && (checkBox3.Checked == true) && (checkBox1.Checked == false))
              {
                  LineItem myCurve = myPane.AddCurve("V1", list_V2, Color.Green, SymbolType.None);
                  LineItem myCurve1 = myPane.AddCurve("V2", list_V3, Color.Blue, SymbolType.None);
              }
              if ((checkBox1.Checked == true) && (checkBox3.Checked == true) && (checkBox2.Checked == true))
              {
                  LineItem myCurve1 = myPane.AddCurve("Vb", list_V1, Color.Red, SymbolType.None);
                  LineItem myCurve2 = myPane.AddCurve("V1", list_V2, Color.Green, SymbolType.None);
                  LineItem myCurve3 = myPane.AddCurve("V2", list_V3, Color.Blue, SymbolType.None);
              }
              
          }
          if (radioButton2.Checked == true)
          {
              if (checkBox6.Checked == true)
              {
                  LineItem myCurve = myPane.AddCurve("Ie", list_I, Color.Blue, SymbolType.None);
              }
             
          }
          if (energy.Checked == true)
          {
              LineItem myCurve3 = myPane.AddCurve("Power", list_P, Color.Fuchsia, SymbolType.None);
              myCurve3.IsY2Axis = true;   //使联系到Y2轴                           
          }
            
          zedGraphControl1.AxisChange();
          zedGraphControl1.Refresh();
          zedGraphControl1.Invalidate();
      }

      private double x_data_count(double i)
      {
          i = i + 0.005;
        
          return i;
      }
      private void insert_sql()
      {

              login set = new login();
              set.ShowDialog();    //这个只能显示一个窗体        
              string name = set.test_name;
              string type = set.test_car_type;
              string quality = set.test_quality;
              string people = set.test_people;
              if ((name != null) && (type != null) && (quality != null) && (people != null) && (name != "") && (type != "") && (quality != "") && (people != ""))
              {
                  if (energy.Checked == true)
                  {
                      string[] vol_1 = V1_array.ToArray();
                      string[] vol_2 = V2_array.ToArray();
                      string[] vol_3 = V3_array.ToArray();
                      string[] cur_1 = I1_array.ToArray();
                      string[] pow_1 = P_array.ToArray();

                      string str_vol_1 = string.Join(";", vol_1);
                      string str_vol_2 = string.Join(";", vol_2);
                      string str_vol_3 = string.Join(";", vol_3);
                      string str_cur_1 = string.Join(";", cur_1);
                      string str_pow_1 = string.Join(";", pow_1);

                      DateTime now1 = DateTime.Now;
                      string nowtime = now1.ToString("yyyy-MM-dd HH:mm:ss");
                      string str_insert = "insert into data(datetime,vol_1,vol_2,vol_3,I_1,Power,people,company,car_type,quality";
                      str_insert += ") values (";
                      str_insert += "'" + nowtime;//时间
                      str_insert += "','" + str_vol_1;
                      str_insert += "','" + str_vol_2;
                      str_insert += "','" + str_vol_3;
                      str_insert += "','" + str_cur_1;
                      str_insert += "','" + str_pow_1;
                      str_insert += "','" + people;
                      str_insert += "','" + name;
                      str_insert += "','" + type;
                      str_insert += "','" + quality;
                      str_insert += "')";
                      mysql.ExecuteNonQuery(str_insert);
                      MessageBox.Show("能量数据存储成功！");
                      V1_array.Clear();
                      V2_array.Clear();
                      V3_array.Clear();
                      I1_array.Clear();
                      P_array.Clear();
                      R_array.Clear();
                  }
                  else if (RES_radio.Checked == true)
                  {
                      string[] vol_1 = V1_array.ToArray();
                      string[] vol_2 = V2_array.ToArray();
                      string[] vol_3 = V3_array.ToArray();
                      string[] cur_1 = I1_array.ToArray();
                      string[] R = R_array.ToArray();

                      string str_vol_1 = string.Join(";", vol_1);
                      string str_vol_2 = string.Join(";", vol_2);
                      string str_vol_3 = string.Join(";", vol_3);
                      string str_cur_1 = string.Join(";", cur_1);
                      string str_R = string.Join(";", R);

                      DateTime now1 = DateTime.Now;
                      string nowtime = now1.ToString("yyyy-MM-dd HH:mm:ss");
                      string str_insert = "insert into data(datetime,vol_1,vol_2,vol_3,I_1,R,people,company,car_type,quality";
                      str_insert += ") values (";
                      str_insert += "'" + nowtime;//时间
                      str_insert += "','" + str_vol_1;
                      str_insert += "','" + str_vol_2;
                      str_insert += "','" + str_vol_3;
                      str_insert += "','" + str_cur_1;
                      str_insert += "','" + str_R;
                      str_insert += "','" + people;
                      str_insert += "','" + name;
                      str_insert += "','" + type;
                      str_insert += "','" + quality;
                      str_insert += "')";
                      mysql.ExecuteNonQuery(str_insert);//存数据库，insert。
                      MessageBox.Show("电阻值数据存储成功！");
                      V1_array.Clear();
                      V2_array.Clear();
                      V3_array.Clear();
                      I1_array.Clear();
                      P_array.Clear();
                      R_array.Clear();
                  }
                  else
                  {
                      MessageBox.Show("测试无效，无能量或电阻值！");
 
                  }
              }
              else
              {
                  MessageBox.Show("请输入正确信息！");
                  return;
              }
              
              
      }

     
      private void button2_Click(object sender, EventArgs e)//绝缘电阻测试
      {
          string str = ResValue;
          byte[] bytetest=new byte[5];
          byte[] Vb=new byte[4];
          //byte [] VandRes=new byte[9];

          if (ResChoose == 1)
          {
              if ((VbChoose == 1))
              {
                  {
                      zedGraphControl1.GraphPane.CurveList.Clear();
                      Array.Clear(recedata, 0, recedata.Length);
                      Array.Clear(arrRecMsg, 0, arrRecMsg.Length);
                      panel1.Visible = false;
                      

                      button3.Enabled = true;
                      button1.Enabled = false;
                      energy.Checked = false;
                      RES_radio.Checked = true; 
                   try
                   {
                      bytetest = System.Text.Encoding.Default.GetBytes(str.ToString());//将字符转换为字节
                      float a = Convert.ToSingle(Vwork);//电压
                      Vb = BitConverter.GetBytes(a);//电压的字节转换
                    
                      for (int i = 0; i < 4; i++)
                      {
                          VandRes[i] = bytetest[i];
                      }
                      for (int j = 0; j < 4; j++)
                      {
                          VandRes[j + 4] = Vb[j];
                      }
                      //if(comboBox2.Text=)
                      VandRes[9] = 0x41;
                      try
                      {
                          socketClient.Send(VandRes);
                      }
                      catch
                      {
                          DialogResult dr;
                          dr = MessageBox.Show("网络连接不正常");
                      }
                      button2.Text = "正在测试中";
                      button2.Enabled = false;
                      label7.Text = "测试次数：";
                      this.zedGraphControl1.IsShowPointValues = true;//鼠标悬停事件，出现点坐标值
                      GraphPane myPane = zedGraphControl1.GraphPane;
                      zedGraphControl1.PointValueEvent += new ZedGraphControl.PointValueHandler(MyPointValueHandler);//手动添加鼠标悬停节点事件
                      //zedGraphControl1.PanModifierKeys = Keys.None;           //直接可以用鼠标左键点击来拖拽
                      zedGraphControl1.GraphPane.XAxis.Scale.MaxAuto = true;
                      zedGraphControl1.GraphPane.XAxis.Scale.MinAuto = true;
                      zedGraphControl1.GraphPane.YAxis.Scale.MaxAuto = true;
                      zedGraphControl1.GraphPane.YAxis.Scale.MinAuto = true;

                      zedGraphControl1.GraphPane.XAxis.Title.Text = "测试次数";

                      zedGraphControl1.GraphPane.XAxis.Type = ZedGraph.AxisType.Linear;
                      zedGraphControl1.GraphPane.Title.Text = "绝缘电阻";
                      zedGraphControl1.GraphPane.YAxis.Title.Text = "绝缘电阻阻值(Ω/V)";
                      zedGraphControl1.GraphPane.XAxis.Scale.Min = 0;
                      zedGraphControl1.GraphPane.Y2Axis.IsVisible = false;   // 使Y2轴不可见  

                      LineItem myCurve = myPane.AddCurve("Res", list_dianzu, Color.DeepPink, SymbolType.None);

                      zedGraphControl1.AxisChange();
                      zedGraphControl1.Refresh();
                      zedGraphControl1.Invalidate();
                      }
                      catch
                      {
                          DialogResult dr;
                          dr = MessageBox.Show("电阻值或工作电压值未知！！！", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                      }
                  }
              }
              else
              {
                  DialogResult dr;
                  dr = MessageBox.Show("电阻值或工作电压值未知！！！", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

              }
          }
          else
          {
              DialogResult dr;
              dr = MessageBox.Show("电阻值或工作电压值未知！！！", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

          }

      }

      private void timer2_Tick(object sender, EventArgs e)//定时1s向下位机发送FF，检查连接状态。若又出现未连接，继续打开定时器1，扫描网络连接。
      {
          byte[] buf = new byte[1]{0xFF};
          try
          {
              socketClient.Send(buf);
          }
          catch
          {
              timer2.Enabled = false;
              timer1.Start();        
          }
      }
        //
        //电流
        //
      private void checkBox6_CheckedChanged(object sender, EventArgs e)
      {

          //this.zedGraphControl1.RestoreScale(this.zedGraphControl1.GraphPane);  //恢复到原图
          creatGraph();
      }

      private void 历史记录MenuItem1_Click(object sender, EventArgs e)
      {
          H_curve h_c = new H_curve();
          h_c.ShowDialog();    //这个只能显示一个窗体 
      }
        //
        //退出
        //
      private void button4_Click(object sender, EventArgs e)
      {
          Application.Exit();
      }

      private void zedGraphControl1_DoubleClick(object sender, EventArgs e)
      {
          this.zedGraphControl1.RestoreScale(this.zedGraphControl1.GraphPane);  //恢复到原图
      }

      private void RES_radio_CheckedChanged(object sender, EventArgs e)
      {
          //this.zedGraphControl1.RestoreScale(this.zedGraphControl1.GraphPane);  //恢复到原图
          //creatGraph();
      }

      private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
      {
           ResChoose = 1;
           switch (comboBox1.Text)
            {
                case "10K":
                    ResValue = "5010";
                    break;
                case"15K":
                    ResValue = "5015";
                    break;
                case "20K":
                    ResValue = "5020";
                    break;
                case "30K":
                    ResValue = "5030";
                    break;
                case "40K":
                    ResValue = "5040";
                    break;
                case "50K":
                    ResValue = "5050";
                    break;
                case "70K":
                    ResValue = "5070";
                    break;
                case "100K":
                    ResValue = "5100";
                    break;
                case "140K":
                    ResValue = "5140";
                    break;
            }
        
      }

      private void energy_CheckedChanged(object sender, EventArgs e)
      {
          this.zedGraphControl1.RestoreScale(this.zedGraphControl1.GraphPane);  //恢复到原图
          creatGraph();
      }

      private void textBox3_TextChanged(object sender, EventArgs e)//将每次输进去的工作电压值传给Vwork
      {
          VbChoose = 1;
          Vwork = textBox3.Text;
          Float_Vb = Convert.ToSingle(Vwork);//电压
      }

      private void timer1_Tick(object sender, EventArgs e)//定时检查网络状态
      {
          if (timer1.Enabled == true)
          {
              socket_set();//定时扫描网络连接状态
          }
          if (WifiNotlink.Stop_flag == 1)
          {
              timer1.Stop();
              Application.Exit();//双击空白，退出应用。
 
          }
      }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)//单双表测量模式更改事件
        {
            if (comboBox2.Text == "单表")
            {
                VandRes[8] = 0;
            }
            else if (comboBox2.Text == "双表")//双表
            {
                VandRes[8] = 1;
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)//电压只能输入数字
        {
            //if (!(char.IsNumber(e.KeyChar)) && e.KeyChar != 7)           //只能输入数字和退格键
            //{
            //    MessageBox.Show("只能输入数字", "系统提示");
            //    e.Handled = true;                               //经过判断为数字可以输入
            //}
            if ((e.KeyChar < 48 || e.KeyChar > 57) && e.KeyChar != 8 && e.KeyChar != 46)
            {
                MessageBox.Show("只能输入数字", "系统提示");
                e.Handled = true;
            }
            //输入为负号时，只能输入一次且只能输入一次
            if (e.KeyChar == 45 && (((TextBox)sender).SelectionStart != 0 || ((TextBox)sender).Text.IndexOf("-") >= 0)) e.Handled = true;
            if (e.KeyChar == 46 && ((TextBox)sender).Text.IndexOf(".") >= 0) e.Handled = true;
        }

    
    }
}
