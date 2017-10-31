using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using WeChat.NET.Controls;
using WeChat.NET.Objects;
using WeChat.NET.HTTP;
using Newtonsoft.Json.Linq;
using WeChat.NET.caozuo;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Specialized;

namespace WeChat.NET
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }
        //数据库连接语句
        public string connstr = "Data Source =.; Integrated Security = SSPI; Initial Catalog = database";
        public static string ssusername = "";
        public static string usernameid ;
        public static int flagjs = 0;
        public static int taifei = 0;
        //setLabel方法及委托
        delegate void delegateSetLabel(string str);
        void setLabel(string str)
        {
            if (this.InvokeRequired)
            {
                Invoke(new delegateSetLabel(setLabel), new object[] { str });
            }
            else
            {
                textBox1.Text = textBox1.Text + Environment.NewLine + str;
            }
        }
        /// <summary>
        /// 实现数据的四舍五入法
        　　 /// </summary>
        /// <param name="v">要进行处理的数据</param>
        /// <param name="x">保留的小数位数</param>
        /// <returns>四舍五入后的结果</returns>
        private double Round(double v, int x)
        {
            bool isNegative = false;
            //如果是负数
            if (v < 0)
            {
                isNegative = true;
                v = -v;
            }

            int IValue = 1;
            for (int i = 1; i <= x; i++)
            {
                IValue = IValue * 10;
            }
            double Int = Math.Round(v * IValue + 0.5, 0);
            v = Int / IValue;

            if (isNegative)
            {
                v = -v;
            }

            return v;
        }
        //填充listbox
        private void listbox_load()
        {
            wxcaozuo wxcz = new caozuo.wxcaozuo();
            listBox1.Items.Clear();
            SqlConnection myConn = new SqlConnection(connstr);//创建数据库连接类的对象
            myConn.Open(); //将连接打开
            SqlDataAdapter myDataAdapter = new SqlDataAdapter("select * from shishishuju order by flag desc", myConn);
            DataSet myDataSet = new DataSet();
            myDataAdapter.Fill(myDataSet, "shishishuju");
            DataTable myTable = myDataSet.Tables["shishishuju"];



            foreach (DataRow myRow in myTable.Rows)
            {
                List<string> lstname = wxcz.Read();//在1.txt里读取
                foreach (string s in lstname)
                {
                    int i = lstname.IndexOf(s);//i就是下标

                    if (s.ToString().Substring(10, lstname[i].IndexOf("<UserName>") - 10) != myRow["nickname"].ToString())
                    {
                        continue;
                    }
                    if (Convert.ToInt32(myRow["flag"]) > 0)
                    {
                        listBox1.Items.Add("*正常*" + myRow["nickname"].ToString());
                    }
                    else
                    {
                        if (Convert.ToInt32(myRow["flag"]) == 0)
                        {
                            listBox1.Items.Add("*虚拟*" + myRow["nickname"].ToString());
                        }
                        if (Convert.ToInt32(myRow["flag"]) == -1)
                        {
                            listBox1.Items.Add("*隐藏*" + myRow["nickname"].ToString());
                        }
                    }
                }
            }
            myConn.Close();
        }
        private void frmMain_Load(object sender, EventArgs e)
        {
            if (frmGroup.flagchecked == 0)//新开始数据报表
            {
                dataGridView1.Rows.Add("1靴1局", "闲", "庄", "闲对", "庄对", "和");
                dataGridView2.Rows.Add("1靴1局", "本局得分", "剩余分", "初始分");
            }
            else//继续上次的数据报表
            {
                SqlConnection myConn = new SqlConnection(connstr);//创建数据库连接类的对象
                myConn.Open(); //将连接打开
                SqlDataAdapter myDataAdapter = new SqlDataAdapter("select * from youxijilu where timestamp = '" + frmGroup.timestamp + "'order by xue desc", myConn);
                DataSet myDataSet = new DataSet();
                myDataAdapter.Fill(myDataSet, "youxijilu");
                DataTable myTable = myDataSet.Tables["youxijilu"];
                if (myDataSet.Tables[0].Rows.Count == 0)
                {
                    dataGridView1.Rows.Add("1靴1局", "闲", "庄", "闲对", "庄对", "和");
                    dataGridView2.Rows.Add("1靴1局", "本局得分", "剩余分", "初始分");
                }
                else
                {
                    foreach (DataRow myRow in myTable.Rows)
                    {
                        SqlDataAdapter myDataAdapter1 = new SqlDataAdapter("select * from youxijilu where timestamp = '" + frmGroup.timestamp + "'and xue ='"+ myRow ["xue"] + "'order by kou desc ", myConn);
                        DataSet myDataSet1 = new DataSet();
                        myDataAdapter1.Fill(myDataSet1, "youxijilu");
                        DataTable myTable1 = myDataSet1.Tables["youxijilu"];
                        foreach (DataRow myRow1 in myTable1.Rows)
                        {
                            dataGridView1.Rows.Add( myRow["xue"] + "靴" + (Convert.ToInt32(myRow1["kou"])+1) + "局", "闲", "庄", "闲对", "庄对", "和");
                            dataGridView2.Rows.Add(myRow["xue"] + "靴" + (Convert.ToInt32(myRow1["kou"] )+1)+ "局", "本局得分", "剩余分", "初始分");
                            break;
                        }
                        break;
                    }
                }
            }

            //第一次建立datagridview
            dataGridView1[0, 0].Style.BackColor = Color.Yellow;
            dataGridView1[1, 0].Style.BackColor = Color.DarkGray;
            dataGridView1[2, 0].Style.BackColor = Color.Gray;
            dataGridView1[3, 0].Style.BackColor = Color.SlateBlue;
            dataGridView1[4, 0].Style.BackColor = Color.Pink;
            dataGridView1[5, 0].Style.BackColor = Color.DarkGray;
            
            dataGridView2[0, 0].Style.BackColor = Color.Yellow;
            dataGridView2[1, 0].Style.BackColor = Color.DarkGray;
            dataGridView2[2, 0].Style.BackColor = Color.Gray;
            dataGridView2[3, 0].Style.BackColor = Color.SlateBlue;
            dataGridView3.Rows.Add("共计20名", "0", "0", "0", "0", "0");
            dataGridView3[0, 0].Style.BackColor = Color.Yellow;
            dataGridView3[1, 0].Style.BackColor = Color.DarkGray;
            dataGridView3[2, 0].Style.BackColor = Color.Gray;
            dataGridView3[3, 0].Style.BackColor = Color.SlateBlue;
            dataGridView3[4, 0].Style.BackColor = Color.Pink;
            dataGridView3[5, 0].Style.BackColor = Color.DarkGray;
            dataGridView4.Rows.Add("共计20名", "0", "0", "0");
            dataGridView4[0, 0].Style.BackColor = Color.Yellow;
            dataGridView4[1, 0].Style.BackColor = Color.DarkGray;
            dataGridView4[2, 0].Style.BackColor = Color.Gray;
            dataGridView4[3, 0].Style.BackColor = Color.SlateBlue;
            wxcaozuo wxcz1 = new wxcaozuo();
            taifei = Convert .ToInt32 ( wxcz1.IniReadValue("系统设置", "台费", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini"));
            toolStripStatusLabel2.Text = taifei.ToString();
            List<string> lstname = wxcz1.Read();//在1.txt里读取
            int o = 1;
            foreach (string s in lstname)
            {
                int i = lstname.IndexOf(s);//i就是下标
                                           //设置字体颜色
                //填充datagridview2
                SqlConnection myConn = new SqlConnection(connstr);//创建数据库连接类的对象
                myConn.Open(); //将连接打开
                SqlDataAdapter myDataAdapter = new SqlDataAdapter("select * from shishishuju where nickname= '" + lstname[i].Substring(10, lstname[i].IndexOf("<UserName>") - 10)+ "'", myConn);
                DataSet myDataSet = new DataSet();
                myDataAdapter.Fill(myDataSet, "shishishuju");
                DataTable myTable = myDataSet.Tables["shishishuju"];
                if (myDataSet.Tables[0].Rows.Count == 0)
                {
                    DataRow myRow = myTable.NewRow();
                    myRow["nickname"] = lstname[i].Substring(10, lstname[i].IndexOf("<UserName>") - 10);//用户
                    myRow["chushifenshu"] = 0;//初始分数
                    myRow["shenyufenshu"] = 0;//剩余分数
                    myRow["benjudengfen"] = 0;//本局得分
                    myRow["flag"] = 1;
                    myTable.Rows.Add(myRow);
                    dataGridView2.Rows.Add(myRow["nickname"].ToString(), myRow["benjudengfen"].ToString(), myRow["shenyufenshu"].ToString(), myRow["chushifenshu"].ToString());
                    SqlCommandBuilder mySqlCommandBuilder = new SqlCommandBuilder(myDataAdapter);
                    myDataAdapter.Update(myDataSet, "shishishuju");
                    myDataSet.Clear();
                    dataGridView1.Rows.Add(lstname[i].Substring(10, lstname[i].IndexOf("<UserName>") - 10), 0, 0, 0, 0, 0);
                    dataGridView1[0, o].Style.ForeColor = Color.Gray;
                    o += 1;
                }
                else
                {

                    foreach (DataRow myRow in myTable.Rows)
                    {
                        if ((Convert.ToInt32(myRow["flag"]) >= 0) )
                        {
                            //textBox10.Text = myRow["yueyhk"].ToString();
                            dataGridView2.Rows.Add(myRow["nickname"].ToString(), myRow["benjudengfen"].ToString(), myRow["shenyufenshu"].ToString(), myRow["chushifenshu"].ToString());
                            dataGridView4[1, 0].Value = Convert.ToInt32(dataGridView4[1, 0].Value) + Convert.ToInt32(myRow["benjudengfen"].ToString());
                            dataGridView4[2, 0].Value = Convert.ToInt32(dataGridView4[2, 0].Value) + Convert.ToInt32(myRow["shenyufenshu"].ToString());
                            dataGridView4[3, 0].Value = Convert.ToInt32(dataGridView4[3, 0].Value) + Convert.ToInt32(myRow["chushifenshu"].ToString());
                            dataGridView1.Rows.Add(lstname[i].Substring(10, lstname[i].IndexOf("<UserName>") - 10), 0, 0, 0, 0, 0);
                            dataGridView1[0, o].Style.ForeColor = Color.Gray;
                            o += 1;
                        }
                    }
                    
                }
                myConn.Close();
                dataGridView3[0, 0].Value = "共计" + (o-1) + "名";
                dataGridView4[0, 0].Value = "共计" + (o-1) + "名";
            }








            listbox_load();





            ((Action)(delegate ()
            {
                WXService wxs = new WXService();
                wxcaozuo wxcz = new wxcaozuo();
                JObject init_result = wxs.WxInit();  //初始化
                usernameid = init_result["User"]["UserName"].ToString();
                string sync_flag = "";
                JObject sync_result;
                while (true)
                {
                    sync_flag = wxs.WxSyncCheck();  //同步检查
                    if (sync_flag == null)
                    {
                        continue;
                    }
                    //这里应该判断 sync_flag中selector的值
                    else //有消息
                    {
                        sync_result = wxs.WxSync();  //进行同步
                        if (sync_result != null)
                        {
                            if (sync_result["AddMsgCount"] != null && sync_result["AddMsgCount"].ToString() != "0")
                            {
                                foreach (JObject m in sync_result["AddMsgList"])
                                {
                                    string from = m["FromUserName"].ToString();
                                    string to = m["ToUserName"].ToString();
                                    string content = m["Content"].ToString();
                                    string type = m["MsgType"].ToString();

                                    WXMsg msg = new WXMsg();
                                    msg.From = from;
                                    //msg.Msg = type == "1" ? content : "请在其他设备上查看消息1111";  //只接受文本消息
                                    msg.Readed = false;
                                    msg.Time = DateTime.Now;
                                    msg.To = to;
                                    msg.Type = int.Parse(type);
                                    msg.Msg = m["Content"].ToString();

                                    if (msg.Type == 51)  //屏蔽一些系统数据
                                    {
                                        continue;
                                    }
                                    if (msg.Type == 47)//屏蔽表情
                                    {
                                        continue;
                                    }
                                    if (msg.Type != 1)
                                    {
                                        continue;
                                    }
                                    //消息处理
                                    if ((msg.To == frmGroup.groupid) || (msg.From == frmGroup.groupid))
                                    {
                                        //消息来自群成员
                                        if (msg.From.IndexOf("@@") >= 0)
                                        {
                                            int flaggs = 0;//标志下注格式是否规范（1为规范，0为不规范,2为不存在成员,3为余额不足,4为未到投注时间）
                                            int dgvindex = 0;
                                            string content1 = msg.Msg.Remove(0, msg.Msg.IndexOf("<br/>") + 5);//获取消息内容
                                            string name = wxcz.getname(msg.Msg.Replace(":<br/>" + content1, ""));//获取发送人
                                            textBox1.Text = textBox1.Text + "消息发自：" + name + "         消息内容:" + content1 + Environment.NewLine;
                                            string ppp = wxcz.guifan(content1);
                                            //获取datagridview操作行
                                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                                            {
                                                if (dataGridView1.Rows[i].Cells[0].Value.ToString() == name)
                                                {
                                                    dgvindex = i;
                                                    break;
                                                }
                                            }
                                            string regex = @"^-?\d+\.?\d*$";
                                            string lll = "";
                                            string ttt = ppp;//为ppp做备份
                                            int kkk = Regex.Matches(ttt, @"：").Count;
                                            //对datagridview赋值
                                            try
                                            {
                                                for (int j = 0; j < kkk; j++)
                                                {
                                                    lll = ppp.Substring(ppp.LastIndexOf("|"), ppp.Length - ppp.LastIndexOf("|"));//从右边取出符合格式的字符串
                                                    switch (lll.Substring(lll.IndexOf("|") + 1, lll.IndexOf("：") - lll.IndexOf("|") - 1))
                                                    {
                                                        case "闲":
                                                            if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                            {
                                                                if (Convert.ToInt32(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "").ToString()) <= Convert.ToInt32(dataGridView2[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[3, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[4, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[5, dgvindex].Value.ToString()))
                                                                {
                                                                    flaggs = 1;
                                                                }
                                                                else
                                                                {
                                                                    flaggs = 3;
                                                                }
                                                            };
                                                            break;
                                                        case "庄":
                                                            if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                            {
                                                                if (Convert.ToInt32(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "").ToString()) <= Convert.ToInt32(dataGridView2[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[3, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[4, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[5, dgvindex].Value.ToString()))
                                                                {
                                                                    flaggs = 1;
                                                                }
                                                                else
                                                                {
                                                                    flaggs = 3;
                                                                }
                                                            };
                                                            break;
                                                        case "闲对":
                                                            if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                            {
                                                                if (Convert.ToInt32(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "").ToString()) <= Convert.ToInt32(dataGridView2[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[1, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[4, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[5, dgvindex].Value.ToString()))
                                                                {
                                                                    flaggs = 1;
                                                                }
                                                                else
                                                                {
                                                                    flaggs = 3;
                                                                }
                                                            };
                                                            break;
                                                        case "庄对":
                                                            if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                            {
                                                                if (Convert.ToInt32(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "").ToString()) <= Convert.ToInt32(dataGridView2[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[1, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[3, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[5, dgvindex].Value.ToString()))
                                                                {
                                                                    flaggs = 1;
                                                                }
                                                                else
                                                                {
                                                                    flaggs = 3;
                                                                }
                                                            };
                                                            break;
                                                        case "和":
                                                            if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                            {
                                                                if (Convert.ToInt32(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "").ToString()) <= Convert.ToInt32(dataGridView2[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[1, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[3, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[4, dgvindex].Value.ToString()))
                                                                {
                                                                    flaggs = 1;
                                                                }
                                                                else
                                                                {
                                                                    flaggs = 3;
                                                                }
                                                            };
                                                            break;
                                                        case "三宝":
                                                            if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                            {
                                                                if (Convert.ToInt32(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "").ToString()) * 3 <= Convert.ToInt32(dataGridView2[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[1, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[2, dgvindex].Value.ToString()))
                                                                {
                                                                    flaggs = 1;
                                                                }
                                                                else
                                                                {
                                                                    flaggs = 3;
                                                                }
                                                            };
                                                            break;
                                                        case "撤":
                                                            flaggs = 1;
                                                            break;
                                                        default:
                                                            flaggs = 0; break;//标记下注信息有异常
                                                    }
                                                    ppp = ppp.Remove(ppp.LastIndexOf("|"), ppp.Length - ppp.LastIndexOf("|"));
                                                }
                                            }
                                            catch
                                            {
                                                flaggs = 0;
                                            }
                                            //梭哈
                                            if (ttt.IndexOf("|梭：") >= 0)
                                            {
                                                string uuu = ttt.Replace("|梭：", "");
                                                switch (uuu)
                                                {
                                                    case "|闲：":
                                                        if (dataGridView2[2, dgvindex].Value.ToString() == "0")
                                                        {
                                                            flaggs = 3;
                                                        }
                                                        else
                                                        {
                                                            flaggs = 1;
                                                        }
                                                        break;
                                                    case "|庄：":
                                                        if (dataGridView2[2, dgvindex].Value.ToString() == "0")
                                                        {
                                                            flaggs = 3;
                                                        }
                                                        else
                                                        {
                                                            flaggs = 1;
                                                        }
                                                        break;
                                                    case "|闲对：":
                                                        if (dataGridView2[2, dgvindex].Value.ToString() == "0")
                                                        {
                                                            flaggs = 3;
                                                        }
                                                        else
                                                        {
                                                            flaggs = 1;
                                                        }
                                                        break;
                                                    case "|庄对：":
                                                        if (dataGridView2[2, dgvindex].Value.ToString() == "0")
                                                        {
                                                            flaggs = 3;
                                                        }
                                                        else
                                                        {
                                                            flaggs = 1;
                                                        }
                                                        break;
                                                    case "|和：":
                                                        if (dataGridView2[2, dgvindex].Value.ToString() == "0")
                                                        {
                                                            flaggs = 3;
                                                        }
                                                        else
                                                        {
                                                            flaggs = 1;
                                                        }
                                                        break;
                                                    default:
                                                        flaggs = 0;
                                                        break;
                                                }
                                            }
                                            //判断消息是否符合下注格式
                                            //wxcz.IniReadValue("系统设置", "格式错误", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
                                            if (flaggs != 1)//标志下注格式是否规范（1为规范，0为不规范,2为不存在成员，3为余额不足，4为未到投注时间）
                                            {
                                                //错误格式处理区
                                                switch (flaggs)
                                                {
                                                    case 0:
                                                        if (wxcz.IniReadValue("系统设置", "格式错误", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini") != "")
                                                        {
                                                            wxs.SendMsg("@" + name + wxcz.IniReadValue("系统设置", "格式错误", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini"), usernameid, frmGroup.groupid, 1);
                                                        }
                                                        break;
                                                    case 2:
                                                        if (wxcz.IniReadValue("系统设置", "添加成员", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini") != "")
                                                        {
                                                            wxs.SendMsg("@" + name + wxcz.IniReadValue("系统设置", "添加成员", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini"), usernameid, frmGroup.groupid, 1);
                                                        }
                                                        break;
                                                    case 3:
                                                        if (wxcz.IniReadValue("系统设置", "余额不足", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini") != "")
                                                        {
                                                            wxs.SendMsg("@" + name + wxcz.IniReadValue("系统设置", "余额不足", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini"), usernameid, frmGroup.groupid, 1);
                                                        }
                                                        break;
                                                }

                                                continue;
                                            }
                                            else
                                            {
                                                if (flagjs == 0)//未到下注时间
                                                {
                                                    if (wxcz.IniReadValue("系统设置", "未到下注时间", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini") != "")
                                                    {
                                                        wxs.SendMsg("@" + name + wxcz.IniReadValue("系统设置", "未到下注时间", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini"), usernameid, frmGroup.groupid, 1);
                                                    }
                                                        continue;
                                                }
                                            }
                                            //真正赋值
                                            //梭
                                            if (ttt.IndexOf("|梭：") >= 0)
                                            {
                                                string uuu = ttt.Replace("|梭：", "");
                                                switch (uuu)
                                                {
                                                    case "|闲：":
                                                        dataGridView1[1, dgvindex].Value = Convert.ToInt32(dataGridView2[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[3, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[4, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[5, dgvindex].Value.ToString());
                                                        dataGridView1[2, dgvindex].Value = 0;
                                                        dataGridView1[0, dgvindex].Style.ForeColor = Color.Black;
                                                        break;
                                                    case "|庄：":
                                                        dataGridView1[1, dgvindex].Value = 0;
                                                        dataGridView1[2, dgvindex].Value = Convert.ToInt32(dataGridView2[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[3, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[4, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[5, dgvindex].Value.ToString());
                                                        dataGridView1[0, dgvindex].Style.ForeColor = Color.Black;
                                                        break;
                                                    case "|闲对：":
                                                        dataGridView1[3, dgvindex].Value = Convert.ToInt32(dataGridView2[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[3, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[4, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[5, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[1, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[2, dgvindex].Value.ToString());
                                                        dataGridView1[0, dgvindex].Style.ForeColor = Color.Black;
                                                        break;
                                                    case "|庄对：":
                                                        dataGridView1[4, dgvindex].Value = Convert.ToInt32(dataGridView2[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[3, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[4, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[5, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[1, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[2, dgvindex].Value.ToString());
                                                        dataGridView1[0, dgvindex].Style.ForeColor = Color.Black;
                                                        break;
                                                    case "|和：":
                                                        dataGridView1[5, dgvindex].Value = Convert.ToInt32(dataGridView2[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[3, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[4, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[5, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[1, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[2, dgvindex].Value.ToString());
                                                        dataGridView1[0, dgvindex].Style.ForeColor = Color.Black;
                                                        break;
                                                    default:
                                                        break;
                                                }

                                            }
                                            //普通下注模式
                                            for (int j = 0; j < kkk; j++)
                                            {
                                                lll = ttt.Substring(ttt.LastIndexOf("|"), ttt.Length - ttt.LastIndexOf("|"));
                                                switch (lll.Substring(lll.IndexOf("|") + 1, lll.IndexOf("：") - lll.IndexOf("|") - 1))
                                                {
                                                    case "闲":
                                                        if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                        {
                                                            if (dataGridView1[2, dgvindex].Value.ToString() != "0")
                                                            {
                                                                dataGridView1[2, dgvindex].Value = 0;
                                                            }
                                                            dataGridView1[1, dgvindex].Value = lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "");
                                                            dataGridView1[0, dgvindex].Style.ForeColor = Color.Black;
                                                        };
                                                        break;
                                                    case "庄":
                                                        if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                        {
                                                            if (dataGridView1[1, dgvindex].Value.ToString() != "0")
                                                            {
                                                                dataGridView1[1, dgvindex].Value = 0;
                                                            }
                                                            dataGridView1[2, dgvindex].Value = lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "");
                                                            dataGridView1[0, dgvindex].Style.ForeColor = Color.Black;
                                                        };
                                                        break;
                                                    case "闲对":
                                                        if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                        {
                                                            dataGridView1[3, dgvindex].Value = lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "");
                                                            dataGridView1[0, dgvindex].Style.ForeColor = Color.Black;
                                                        };
                                                        break;
                                                    case "庄对":
                                                        if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                        {
                                                            dataGridView1[4, dgvindex].Value = lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "");
                                                            dataGridView1[0, dgvindex].Style.ForeColor = Color.Black;
                                                        };
                                                        break;
                                                    case "和":
                                                        if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                        {
                                                            dataGridView1[5, dgvindex].Value = lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "");
                                                            dataGridView1[0, dgvindex].Style.ForeColor = Color.Black;
                                                        };
                                                        break;
                                                    case "三宝":
                                                        if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                        {
                                                            dataGridView1[3, dgvindex].Value = lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "");
                                                            dataGridView1[4, dgvindex].Value = lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "");
                                                            dataGridView1[5, dgvindex].Value = lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "");
                                                            dataGridView1[0, dgvindex].Style.ForeColor = Color.Black;
                                                        };
                                                        break;
                                                    case "撤":
                                                        dataGridView1[1, dgvindex].Value = 0;
                                                        dataGridView1[2, dgvindex].Value = 0;
                                                        dataGridView1[3, dgvindex].Value = 0;
                                                        dataGridView1[4, dgvindex].Value = 0;
                                                        dataGridView1[5, dgvindex].Value = 0;
                                                        dataGridView1[0, dgvindex].Style.ForeColor = Color.Gray;
                                                        break;
                                                    default:
                                                        flaggs = 0; break;//标记下注信息有异常
                                                }
                                                ttt = ttt.Remove(ttt.LastIndexOf("|"), ttt.Length - ttt.LastIndexOf("|"));
                                            }
                                        }//end

                                        //消息来自自己
                                        if (msg.From.IndexOf("@@") < 0)
                                        {
                                            int flaggs = 0;//标志下注格式是否规范（1为规范，0为不规范,2为不存在成员，3为余额不足，4为未到投注时间）
                                            int dgvindex = 0;
                                            string content1 = msg.Msg;//获取消息内容
                                            string name = wxcz.getname(msg.From);//获取发送人
                                            textBox1.Text = textBox1.Text + "消息发自：" + name + "         消息内容:" + content1 + Environment.NewLine;
                                            string ppp = wxcz.guifan(msg.Msg);

                                            //获取datagridview操作行
                                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                                            {
                                                if (dataGridView1.Rows[i].Cells[0].Value.ToString() == name)
                                                {
                                                    dgvindex = i;
                                                    break;
                                                }
                                            }
                                            string regex = @"^-?\d+\.?\d*$";
                                            string lll = "";
                                            string ttt = ppp;//为ppp做备份
                                            int kkk = Regex.Matches(ttt, @"：").Count;
                                            //对datagridview赋值
                                            try
                                            {
                                                for (int j = 0; j < kkk; j++)
                                                {
                                                    lll = ppp.Substring(ppp.LastIndexOf("|"), ppp.Length - ppp.LastIndexOf("|"));//从右边取出符合格式的字符串
                                                    switch (lll.Substring(lll.IndexOf("|") + 1, lll.IndexOf("：") - lll.IndexOf("|") - 1))
                                                    {
                                                        case "闲":
                                                            if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                            {
                                                                if (Convert.ToInt32(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "").ToString()) <= Convert.ToInt32(dataGridView2[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[3, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[4, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[5, dgvindex].Value.ToString()))
                                                                {
                                                                    flaggs = 1;
                                                                }
                                                                else
                                                                {
                                                                    flaggs = 3;
                                                                }
                                                            };
                                                            break;
                                                        case "庄":
                                                            if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                            {
                                                                if (Convert.ToInt32(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "").ToString()) <= Convert.ToInt32(dataGridView2[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[3, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[4, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[5, dgvindex].Value.ToString()))
                                                                {
                                                                    flaggs = 1;
                                                                }
                                                                else
                                                                {
                                                                    flaggs = 3;
                                                                }
                                                            };
                                                            break;
                                                        case "闲对":
                                                            if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                            {
                                                                if (Convert.ToInt32(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "").ToString()) <= Convert.ToInt32(dataGridView2[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[1, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[4, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[5, dgvindex].Value.ToString()))
                                                                {
                                                                    flaggs = 1;
                                                                }
                                                                else
                                                                {
                                                                    flaggs = 3;
                                                                }
                                                            };
                                                            break;
                                                        case "庄对":
                                                            if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                            {
                                                                if (Convert.ToInt32(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "").ToString()) <= Convert.ToInt32(dataGridView2[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[1, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[3, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[5, dgvindex].Value.ToString()))
                                                                {
                                                                    flaggs = 1;
                                                                }
                                                                else
                                                                {
                                                                    flaggs = 3;
                                                                }
                                                            };
                                                            break;
                                                        case "和":
                                                            if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                            {
                                                                if (Convert.ToInt32(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "").ToString()) <= Convert.ToInt32(dataGridView2[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[1, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[3, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[4, dgvindex].Value.ToString()))
                                                                {
                                                                    flaggs = 1;
                                                                }
                                                                else
                                                                {
                                                                    flaggs = 3;
                                                                }
                                                            };
                                                            break;
                                                        case "三宝":
                                                            if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                            {
                                                                if (Convert.ToInt32(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "").ToString()) * 3 <= Convert.ToInt32(dataGridView2[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[1, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[2, dgvindex].Value.ToString()))
                                                                {
                                                                    flaggs = 1;
                                                                }
                                                                else
                                                                {
                                                                    flaggs = 3;
                                                                }
                                                            };
                                                            break;
                                                        case "撤":
                                                            flaggs = 1;
                                                            break;
                                                        default:
                                                            flaggs = 0; break;//标记下注信息有异常
                                                    }
                                                    ppp = ppp.Remove(ppp.LastIndexOf("|"), ppp.Length - ppp.LastIndexOf("|"));
                                                }
                                            }
                                            catch
                                            {
                                                flaggs = 0;
                                            }
                                            //梭哈
                                            if (ttt.IndexOf("|梭：") >= 0)
                                            {
                                                string uuu = ttt.Replace("|梭：", "");
                                                switch (uuu)
                                                {
                                                    case "|闲：":
                                                        if (dataGridView2[2, dgvindex].Value.ToString() == "0")
                                                        {
                                                            flaggs = 3;
                                                        }
                                                        else
                                                        {
                                                            flaggs = 1;
                                                        }
                                                        break;
                                                    case "|庄：":
                                                        if (dataGridView2[2, dgvindex].Value.ToString() == "0")
                                                        {
                                                            flaggs = 3;
                                                        }
                                                        else
                                                        {
                                                            flaggs = 1;
                                                        }
                                                        break;
                                                    case "|闲对：":
                                                        if (dataGridView2[2, dgvindex].Value.ToString() == "0")
                                                        {
                                                            flaggs = 3;
                                                        }
                                                        else
                                                        {
                                                            flaggs = 1;
                                                        }
                                                        break;
                                                    case "|庄对：":
                                                        if (dataGridView2[2, dgvindex].Value.ToString() == "0")
                                                        {
                                                            flaggs = 3;
                                                        }
                                                        else
                                                        {
                                                            flaggs = 1;
                                                        }
                                                        break;
                                                    case "|和：":
                                                        if (dataGridView2[2, dgvindex].Value.ToString() == "0")
                                                        {
                                                            flaggs = 3;
                                                        }
                                                        else
                                                        {
                                                            flaggs = 1;
                                                        }
                                                        break;
                                                    default:
                                                        flaggs = 0;
                                                        break;
                                                }
                                            }
                                            //判断消息是否符合下注格式
                                            if (flaggs != 1)//标志下注格式是否规范（1为规范，0为不规范,2为不存在成员，3为余额不足,4为未到投注时间）
                                            {
                                                //错误格式处理区

                                                continue;
                                            }
                                            //真正赋值
                                            //梭
                                            if (ttt.IndexOf("|梭：") >= 0)
                                            {
                                                string uuu = ttt.Replace("|梭：", "");
                                                switch (uuu)
                                                {
                                                    case "|闲：":
                                                        dataGridView1[1, dgvindex].Value = Convert.ToInt32(dataGridView2[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[3, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[4, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[5, dgvindex].Value.ToString());
                                                        dataGridView1[2, dgvindex].Value = 0;
                                                        dataGridView1[0, dgvindex].Style.ForeColor = Color.Black;
                                                        break;
                                                    case "|庄：":
                                                        dataGridView1[1, dgvindex].Value = 0;
                                                        dataGridView1[2, dgvindex].Value = Convert.ToInt32(dataGridView2[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[3, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[4, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[5, dgvindex].Value.ToString());
                                                        dataGridView1[0, dgvindex].Style.ForeColor = Color.Black;
                                                        break;
                                                    case "|闲对：":
                                                        dataGridView1[3, dgvindex].Value = Convert.ToInt32(dataGridView2[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[3, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[4, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[5, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[1, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[2, dgvindex].Value.ToString());
                                                        dataGridView1[0, dgvindex].Style.ForeColor = Color.Black;
                                                        break;
                                                    case "|庄对：":
                                                        dataGridView1[4, dgvindex].Value = Convert.ToInt32(dataGridView2[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[3, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[4, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[5, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[1, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[2, dgvindex].Value.ToString());
                                                        dataGridView1[0, dgvindex].Style.ForeColor = Color.Black;
                                                        break;
                                                    case "|和：":
                                                        dataGridView1[5, dgvindex].Value = Convert.ToInt32(dataGridView2[2, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[3, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[4, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[5, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[1, dgvindex].Value.ToString()) - Convert.ToInt32(dataGridView1[2, dgvindex].Value.ToString());
                                                        dataGridView1[0, dgvindex].Style.ForeColor = Color.Black;
                                                        break;
                                                    default:
                                                        flaggs = 0;
                                                        break;
                                                }

                                            }
                                            //普通下注模式
                                            for (int j = 0; j < kkk; j++)
                                            {
                                                lll = ttt.Substring(ttt.LastIndexOf("|"), ttt.Length - ttt.LastIndexOf("|"));
                                                switch (lll.Substring(lll.IndexOf("|") + 1, lll.IndexOf("：") - lll.IndexOf("|") - 1))
                                                {
                                                    case "闲":
                                                        if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                        {
                                                            if (dataGridView1[2, dgvindex].Value.ToString() != "0")
                                                            {
                                                                dataGridView1[2, dgvindex].Value = 0;
                                                            }
                                                            dataGridView1[1, dgvindex].Value = lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "");
                                                            dataGridView1[0, dgvindex].Style.ForeColor = Color.Black;
                                                        };
                                                        break;
                                                    case "庄":
                                                        if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                        {
                                                            if (dataGridView1[1, dgvindex].Value.ToString() != "0")
                                                            {
                                                                dataGridView1[1, dgvindex].Value = 0;
                                                            }
                                                            dataGridView1[2, dgvindex].Value = lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "");
                                                            dataGridView1[0, dgvindex].Style.ForeColor = Color.Black;
                                                        };
                                                        break;
                                                    case "闲对":
                                                        if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                        {
                                                            dataGridView1[3, dgvindex].Value = lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "");
                                                            dataGridView1[0, dgvindex].Style.ForeColor = Color.Black;
                                                        };
                                                        break;
                                                    case "庄对":
                                                        if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                        {
                                                            dataGridView1[4, dgvindex].Value = lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "");
                                                            dataGridView1[0, dgvindex].Style.ForeColor = Color.Black;
                                                        };
                                                        break;
                                                    case "和":
                                                        if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                        {
                                                            dataGridView1[5, dgvindex].Value = lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "");
                                                            dataGridView1[0, dgvindex].Style.ForeColor = Color.Black;
                                                        };
                                                        break;
                                                    case "三宝":
                                                        if (Regex.IsMatch(lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), ""), regex))//判断是否是纯数字
                                                        {
                                                            dataGridView1[3, dgvindex].Value = lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "");
                                                            dataGridView1[4, dgvindex].Value = lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "");
                                                            dataGridView1[5, dgvindex].Value = lll.Replace(lll.Substring(lll.IndexOf("|"), lll.IndexOf("：") + 1), "");
                                                            dataGridView1[0, dgvindex].Style.ForeColor = Color.Black;
                                                        };
                                                        break;
                                                    case "撤":
                                                        dataGridView1[1, dgvindex].Value = 0;
                                                        dataGridView1[2, dgvindex].Value = 0;
                                                        dataGridView1[3, dgvindex].Value = 0;
                                                        dataGridView1[4, dgvindex].Value = 0;
                                                        dataGridView1[5, dgvindex].Value = 0;
                                                        dataGridView1[0, dgvindex].Style.ForeColor = Color.Gray;
                                                        break;
                                                    default:
                                                        flaggs = 0; break;//标记下注信息有异常
                                                }
                                                ttt = ttt.Remove(ttt.LastIndexOf("|"), ttt.Length - ttt.LastIndexOf("|"));
                                            }
                                        }//end
                                    }
                                    dataGridView3[1, 0].Value = 0;
                                    dataGridView3[2, 0].Value = 0;
                                    dataGridView3[3, 0].Value = 0;
                                    dataGridView3[4, 0].Value = 0;
                                    dataGridView3[5, 0].Value = 0;
                                    for (int i = 1; i < dataGridView1.Rows.Count; i++)
                                    {
                                        dataGridView3[1, 0].Value = Convert.ToInt32(dataGridView3[1, 0].Value) + Convert.ToInt32(dataGridView1[1, i].Value);
                                        dataGridView3[2, 0].Value = Convert.ToInt32(dataGridView3[2, 0].Value) + Convert.ToInt32(dataGridView1[2, i].Value);
                                        dataGridView3[3, 0].Value = Convert.ToInt32(dataGridView3[3, 0].Value) + Convert.ToInt32(dataGridView1[3, i].Value);
                                        dataGridView3[4, 0].Value = Convert.ToInt32(dataGridView3[4, 0].Value) + Convert.ToInt32(dataGridView1[4, i].Value);
                                        dataGridView3[5, 0].Value = Convert.ToInt32(dataGridView3[5, 0].Value) + Convert.ToInt32(dataGridView1[5, i].Value);
                                    }
                                }
                            }
                        }
                        System.Threading.Thread.Sleep(10);
                    }
                }
            })).BeginInvoke(null, null);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)//不能在下注过程中点击
        {
            panel8.Enabled = false;
            button2.Text = "刷新中。。";
            if (frmGroup.groupid == "")
            {
                return;
            }
            WXService wxs = new WXService();
            wxcaozuo wxcz = new wxcaozuo();
            wxcz.Delete();
            JObject contact_result = wxs.GetGroupItem(frmGroup.groupid); //群聊
            if (contact_result != null)
            {
                foreach (JObject contactlist in contact_result["ContactList"])  //完整好友名单
                {
                    foreach (JObject contact in contactlist["MemberList"])  //完整好友名单
                    {
                        WXUser user = new WXUser();
                        wxcz.Write("<NickName>" + contact["NickName"].ToString() + "<UserName>" + contact["UserName"].ToString() + Environment.NewLine, "Append");
                    }
                }
            }
            List<string> lstname = wxcz.Read();//在1.txt里读取
            int flagk = 1;//(为0表示存在，为1表示不存在)
            int o = 1;
            foreach (string s in lstname)
            {
                int i = lstname.IndexOf(s);//i就是下标
                flagk = 1;
                for (int j = 1; j < dataGridView1.RowCount; j++)
                {
                    if (s.ToString ().Substring(10, lstname[i].IndexOf("<UserName>") - 10) == dataGridView1[0, j].Value.ToString())
                    {
                        flagk = 0;
                        break;
                    }
                }
                if (flagk == 1)
                {
                    //填充datagridview2
                    SqlConnection myConn = new SqlConnection(connstr);//创建数据库连接类的对象
                    myConn.Open(); //将连接打开
                    SqlDataAdapter myDataAdapter = new SqlDataAdapter("select * from shishishuju where nickname= '" + lstname[i].Substring(10, lstname[i].IndexOf("<UserName>") - 10) + "'", myConn);
                    DataSet myDataSet = new DataSet();
                    myDataAdapter.Fill(myDataSet, "shishishuju");
                    DataTable myTable = myDataSet.Tables["shishishuju"];
                    if (myDataSet.Tables[0].Rows.Count == 0)
                    {
                        DataRow myRow = myTable.NewRow();
                        myRow["nickname"] = lstname[i].Substring(10, lstname[i].IndexOf("<UserName>") - 10);//用户
                        myRow["chushifenshu"] = 0;//初始分数
                        myRow["shenyufenshu"] = 0;//剩余分数
                        myRow["benjudengfen"] = 0;//本局得分
                        myRow["flag"] = 1;
                        myTable.Rows.Add(myRow);
                        dataGridView2.Rows.Add(myRow["nickname"].ToString(), myRow["benjudengfen"].ToString(), myRow["shenyufenshu"].ToString(), myRow["chushifenshu"].ToString());
                        SqlCommandBuilder mySqlCommandBuilder = new SqlCommandBuilder(myDataAdapter);
                        myDataAdapter.Update(myDataSet, "shishishuju");
                        myDataSet.Clear();
                        dataGridView1.Rows.Add(lstname[i].Substring(10, lstname[i].IndexOf("<UserName>") - 10), 0, 0, 0, 0, 0);
                        dataGridView1[0, dataGridView1.Rows.Count - 1].Style.ForeColor = Color.Gray;
                        o += 1;
                    }
                    else
                    {
                        foreach (DataRow myRow in myTable.Rows)
                        {
                            if ((Convert.ToInt32(myRow["flag"]) >= 0))
                            {
                                //textBox10.Text = myRow["yueyhk"].ToString();
                                dataGridView2.Rows.Add(myRow["nickname"].ToString(), myRow["benjudengfen"].ToString(), myRow["shenyufenshu"].ToString(), myRow["chushifenshu"].ToString());
                                dataGridView4[1, 0].Value = Convert.ToInt32(dataGridView4[1, 0].Value) + Convert.ToInt32(myRow["benjudengfen"].ToString());
                                dataGridView4[2, 0].Value = Convert.ToInt32(dataGridView4[2, 0].Value) + Convert.ToInt32(myRow["shenyufenshu"].ToString());
                                dataGridView4[3, 0].Value = Convert.ToInt32(dataGridView4[3, 0].Value) + Convert.ToInt32(myRow["chushifenshu"].ToString());
                                dataGridView1.Rows.Add(lstname[i].Substring(10, lstname[i].IndexOf("<UserName>") - 10), 0, 0, 0, 0, 0);
                                dataGridView1[0, dataGridView1.Rows.Count - 1].Style.ForeColor = Color.Gray;
                                o += 1;
                            }
                        }
                    }
                    myConn.Close();
                    dataGridView3[0, 0].Value = "共计" + (dataGridView1.Rows.Count - 1) + "名";
                    dataGridView4[0, 0].Value = "共计" + (dataGridView1.Rows.Count - 1) + "名";
                }
            }
            for (int j = 1; j < dataGridView1.RowCount; j++)
            {
                flagk = 1;//(1不存在。0存在)
                foreach (string s in lstname)
                {
                    int i = lstname.IndexOf(s);//i就是下标
                    if (s.ToString().Substring(10, lstname[i].IndexOf("<UserName>") - 10) == dataGridView1[0, j].Value.ToString())
                    {
                        flagk = 0;
                        break;
                    }
                }
                if (flagk == 1)
                {
                    dataGridView1.Rows.RemoveAt(j);
                    dataGridView2.Rows.RemoveAt(j);
                    dataGridView3[0, 0].Value = "共计" + (Convert .ToInt32 ( dataGridView3[0, 0].Value.ToString().Replace ("共计","").Replace ("名","")) - 1) + "名";
                    dataGridView4[0, 0].Value = "共计" + (Convert .ToInt32 ( dataGridView3[0, 0].Value.ToString().Replace ("共计","").Replace ("名","")) - 1) + "名";
                }
            }
            listbox_load();
            button2.Text = "刷新用户";
        }
        private void button7_Click(object sender, EventArgs e)//隐藏用户
        {
            string strrrr = listBox1.SelectedItem.ToString().Replace("*隐藏*", "").Replace("*正常*", "").Replace("*虚拟*", "");
            panel8.Enabled = false;
            SqlConnection myConn = new SqlConnection(connstr);//创建数据库连接类的对象
            myConn.Open(); //将连接打开
            SqlDataAdapter myDataAdapter = new SqlDataAdapter("select * from shishishuju where nickname= '" + strrrr + "'", myConn);
            DataSet myDataSet = new DataSet();
            myDataAdapter.Fill(myDataSet, "shishishuju");
            DataTable myTable = myDataSet.Tables["shishishuju"];
            if (myDataSet.Tables[0].Rows.Count == 0)
            { }
            foreach (DataRow myRow in myTable.Rows)
            {
                myRow["flag"] = -1;
            }
            SqlCommandBuilder mySqlCommandBuilder = new SqlCommandBuilder(myDataAdapter);
            myDataAdapter.Update(myDataSet, "shishishuju");
            myDataSet.Clear();
            myConn.Close();
            
            for (int i = 1; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1[0, i].Value.ToString() == strrrr)
                {
                    dataGridView1.Rows.RemoveAt(i);
                    dataGridView2.Rows.RemoveAt(i);
                }
            }
            button2_Click(sender, e);
           // panel8.Enabled = true;
        }
        private void button6_Click(object sender, EventArgs e)//置顶用户
        {
            string strrrr = listBox1.SelectedItem.ToString().Replace("*隐藏*", "").Replace("*正常*", "").Replace("*虚拟*", "");
            panel8.Enabled = false;
            SqlConnection myConn = new SqlConnection(connstr);//创建数据库连接类的对象
            myConn.Open(); //将连接打开
            SqlDataAdapter myDataAdapter1 = new SqlDataAdapter("select * from shishishuju order by flag desc", myConn);
            SqlDataAdapter myDataAdapter = new SqlDataAdapter("select * from shishishuju where nickname= '" + strrrr + "'", myConn);
            DataSet myDataSet = new DataSet();
            myDataAdapter.Fill(myDataSet, "shishishuju");
            DataTable myTable = myDataSet.Tables["shishishuju"];
            myDataAdapter1.Fill(myDataSet, "111");
            DataTable myTable1 = myDataSet.Tables["111"];
            foreach (DataRow myRow1 in myTable1.Rows)
            {
                foreach (DataRow myRow in myTable.Rows)
                {
                    myRow["flag"] = Convert .ToInt32 ( myRow1["flag"])+1;
                }
                break;
            }
            SqlCommandBuilder mySqlCommandBuilder = new SqlCommandBuilder(myDataAdapter);
            myDataAdapter.Update(myDataSet, "shishishuju");
            myDataSet.Clear();
            myConn.Close();
            button2_Click(sender, e);
        }
        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            int index = e.Index;//获取当前要进行绘制的行的序号，从0开始。
            Graphics g = e.Graphics;//获取Graphics对象。
            Rectangle bound = e.Bounds;//获取当前要绘制的行的一个矩形范围。
            string text = listBox1.Items[index].ToString();//获取当前要绘制的行的显示文本。
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {//如果当前行为选中行。
             //绘制选中时要显示的蓝色边框。
                g.DrawRectangle(Pens.Blue, bound.Left, bound.Top, bound.Width - 1, bound.Height - 1);
                Rectangle rect = new Rectangle(bound.Left+1 , bound.Top+1 ,
                                               bound.Width-2 , bound.Height-2);
                //绘制选中时要显示的蓝色背景。
                g.FillRectangle(Brushes.White, rect);
                //绘制显示文本。
                TextRenderer.DrawText(g, text, this.Font, rect, Color.Blue ,
                                      TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            }
            else
            {
                Color vColor = e.ForeColor;
                switch (listBox1.Items[e.Index].ToString().Substring(0, 4))
                {
                    case "*正常*": vColor = Color.Red; g.FillRectangle(Brushes.Red, bound); break;
                    case "*隐藏*": vColor = Color.Yellow; g.FillRectangle(Brushes.Yellow, bound); break;
                    case "*虚拟*": vColor = Color.Lime; g.FillRectangle(Brushes.Lime, bound); break;
                }
                //TextRenderer.DrawText(g, text, this.Font, bound, vColor,
                //                      TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                e.Graphics.DrawString(((ListBox)sender).Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds);
            }
        }
        private void button5_Click(object sender, EventArgs e)//恢复正常
        {
            string strrrr = listBox1.SelectedItem.ToString().Replace("*隐藏*", "").Replace("*正常*", "").Replace("*虚拟*", "");
            panel8.Enabled = false;
            SqlConnection myConn = new SqlConnection(connstr);//创建数据库连接类的对象
            myConn.Open(); //将连接打开
            SqlDataAdapter myDataAdapter = new SqlDataAdapter("select * from shishishuju where nickname= '" + strrrr + "'", myConn);
            DataSet myDataSet = new DataSet();
            myDataAdapter.Fill(myDataSet, "shishishuju");
            DataTable myTable = myDataSet.Tables["shishishuju"];
            if (myDataSet.Tables[0].Rows.Count == 0)
            { }
            foreach (DataRow myRow in myTable.Rows)
            {
                myRow["flag"] = 1;
            }
            SqlCommandBuilder mySqlCommandBuilder = new SqlCommandBuilder(myDataAdapter);
            myDataAdapter.Update(myDataSet, "shishishuju");
            myDataSet.Clear();
            myConn.Close();
            button2_Click(sender, e);
           // panel8.Enabled = true;
        }
        private void button4_Click(object sender, EventArgs e)//设为虚拟
        {
            string strrrr = listBox1.SelectedItem.ToString().Replace("*隐藏*", "").Replace("*正常*", "").Replace("*虚拟*", "");
            panel8.Enabled = false;
            SqlConnection myConn = new SqlConnection(connstr);//创建数据库连接类的对象
            myConn.Open(); //将连接打开
            SqlDataAdapter myDataAdapter = new SqlDataAdapter("select * from shishishuju where nickname= '" + strrrr + "'", myConn);
            DataSet myDataSet = new DataSet();
            myDataAdapter.Fill(myDataSet, "shishishuju");
            DataTable myTable = myDataSet.Tables["shishishuju"];
            if (myDataSet.Tables[0].Rows.Count == 0)
            { }
            foreach (DataRow myRow in myTable.Rows)
            {
                myRow["flag"] =0;
            }
            SqlCommandBuilder mySqlCommandBuilder = new SqlCommandBuilder(myDataAdapter);
            myDataAdapter.Update(myDataSet, "shishishuju");
            myDataSet.Clear();
            myConn.Close();
            button2_Click(sender, e);
           // panel8.Enabled = true;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            wxcaozuo wxcz = new wxcaozuo();
            int flaggg = 0;
            string strrrr = listBox1.SelectedItem.ToString().Replace("*隐藏*", "").Replace("*正常*", "").Replace("*虚拟*", "");
            panel8.Enabled = false;
            List<string> lstname = wxcz.Read();//在1.txt里读取
            foreach (string s in lstname)
            {
                if (s.ToString().Substring(10,s.IndexOf("<UserName>") - 10) == strrrr)
                { flaggg = 1; }
            }
            if (flaggg == 0)
            {
                using (SqlConnection cn = new SqlConnection(connstr))
                {
                    cn.Open();
                    using (SqlCommand cmd = cn.CreateCommand())
                    {
                        cmd.CommandText = "delete from shishishuju where nickname= '" + strrrr + "'"; //没有指定where条件，就会将所有记录都删除
                        cmd.ExecuteNonQuery();//会返回受影响的行数
                    }
                    cn.Close();
                }
                for (int i = 1; i < dataGridView1.Rows.Count; i++)
                {
                    if (dataGridView1[0, i].Value.ToString() == strrrr)
                    {
                        dataGridView1.Rows.RemoveAt(i);
                        dataGridView2.Rows.RemoveAt(i);
                    }
                }
                MessageBox.Show("删除成功");
            }
            else
            {
                MessageBox.Show ("删除失败！删除只可用于删除不在群聊中的成员！"); 
            }
            button2_Click(sender, e);
            //panel8.Enabled = true;
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            panel8.Enabled = true;
        }
        private void button8_Click(object sender, EventArgs e)//上分
        {
            WXService wxs = new WXService();
            string strrrr = listBox1.SelectedItem.ToString().Replace("*隐藏*", "").Replace("*正常*", "").Replace("*虚拟*", "");
            string str = Interaction.InputBox("正在为---》" + strrrr + "《---上分，请输入分数", "上分", "请输入数字...");
            int result;
            try
            {
                result = Convert.ToInt32(str);
                SqlConnection myConn = new SqlConnection(connstr);//创建数据库连接类的对象
                myConn.Open(); //将连接打开
                SqlDataAdapter myDataAdapter = new SqlDataAdapter("select * from shishishuju where nickname= '" + strrrr + "'", myConn);
                DataSet myDataSet = new DataSet();
                myDataAdapter.Fill(myDataSet, "shishishuju");
                DataTable myTable = myDataSet.Tables["shishishuju"];
                foreach (DataRow myRow in myTable.Rows)
                {
                    myRow["shenyufenshu"] = Convert.ToInt32(myRow["shenyufenshu"]) + result;
                    myRow["chushifenshu"] = Convert.ToInt32(myRow["chushifenshu"]) + result;
                }
                SqlCommandBuilder mySqlCommandBuilder = new SqlCommandBuilder(myDataAdapter);
                myDataAdapter.Update(myDataSet, "shishishuju");
                myDataSet.Clear();
                myConn.Close();
                for (int i = 1; i < dataGridView2.Rows.Count; i++)
                {
                    if (dataGridView2[0, i].Value.ToString() == strrrr)
                    {
                        dataGridView2[2, i].Value = Convert.ToInt32(dataGridView2[2, i].Value) + result;
                        dataGridView2[3, i].Value = Convert.ToInt32(dataGridView2[3, i].Value) + result;
                    }
                }
                dataGridView4[2, 0].Value = Convert.ToInt32(dataGridView4[2, 0].Value) + result;
                dataGridView4[3, 0].Value = Convert.ToInt32(dataGridView4[3, 0].Value) + result;
                MessageBox.Show("已为--->"+ strrrr + "<---成功上分------>"+ result+ "<------");
                wxs.SendMsg("@" + strrrr + " \\r已成功为您上【" + result + "】分！\\r可以开始投注！", usernameid, frmGroup.groupid, 1);
            }
            catch
            {
                MessageBox.Show("操作失败！请输入符合规定的值！");
            }
        }
        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void button9_Click(object sender, EventArgs e)//下分
        {
            string strrrr = listBox1.SelectedItem.ToString().Replace("*隐藏*", "").Replace("*正常*", "").Replace("*虚拟*", "");
            string str = Interaction.InputBox("正在为---》" + strrrr + "《---下分，请输入分数", "下分", "请输入数字...");
            int result;
            try
            {
                result = Convert.ToInt32(str);
                SqlConnection myConn = new SqlConnection(connstr);//创建数据库连接类的对象
                myConn.Open(); //将连接打开
                SqlDataAdapter myDataAdapter = new SqlDataAdapter("select * from shishishuju where nickname= '" + strrrr + "'", myConn);
                DataSet myDataSet = new DataSet();
                myDataAdapter.Fill(myDataSet, "shishishuju");
                DataTable myTable = myDataSet.Tables["shishishuju"];
                foreach (DataRow myRow in myTable.Rows)
                {
                    if ((Convert.ToInt32(myRow["shenyufenshu"]) - result) >= 0)
                    {
                        myRow["shenyufenshu"] = Convert.ToInt32(myRow["shenyufenshu"]) - result;
                        SqlCommandBuilder mySqlCommandBuilder = new SqlCommandBuilder(myDataAdapter);
                        myDataAdapter.Update(myDataSet, "shishishuju");
                        
                        for (int i = 1; i < dataGridView2.Rows.Count; i++)
                        {
                            if (dataGridView2[0, i].Value.ToString() == strrrr)
                            {
                                dataGridView2[2, i].Value = Convert.ToInt32(dataGridView2[2, i].Value) - result;
                            }
                        }
                        dataGridView4[2, 0].Value = Convert.ToInt32(dataGridView4[2, 0].Value) - result;
                        MessageBox.Show("已为--->" + strrrr + "<---成功下分------>" + result + "<------");
                        break ;
                    }
                    else
                    {
                        MessageBox.Show("下分失败！！目标玩家没有这么多分数！");
                    }
                }
                myDataSet.Clear();
                myConn.Close();
            }
            catch
            {
                MessageBox.Show("操作失败！请输入符合规定的值！");
            }
        }
        private void button10_Click(object sender, EventArgs e)
        {
            if (button10.Text == "开始下注")//开始下注事件
            {
                button10.Text = "停止下注";
                flagjs = 1;
                WXService wxs = new WXService();
                wxs.SendMsg("-------开始下注--------", usernameid, frmGroup.groupid, 1);
                wxs.Wx_SendFile_Gif(frmGroup.groupid, "emoji/begin.gif");
                //wxs.Wx_SendFile_Gif(frmGroup.groupid, "C:\\Users\\Administrator\\Desktop\\begin.gif");
                button18.Enabled = true;
                button10.Enabled = false;
                button11.Enabled = false;
                button12.Enabled = false;
                button13.Enabled = false;
                button14.Enabled = false;
            }
            else//停止下注事件
            {
                button10.Text = "开始下注";
                flagjs = 0;
                WXService wxs = new WXService();
                wxs.SendMsg("-------停止下注--------", usernameid, frmGroup.groupid, 1);
                wxs.Wx_SendFile_Gif(frmGroup.groupid, "emoji/stop.gif");
                button18.Enabled = false;
                button11.Enabled = true;
            }
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;                             //最左坐标
            public int Top;                             //最上坐标
            public int Right;                           //最右坐标
            public int Bottom;                        //最下坐标
        }
        private void button11_Click(object sender, EventArgs e)
        {
            int count = dataGridView1.RowCount;
            int y = (count + 1) * 30;
            Bitmap bit = new Bitmap(600, y, PixelFormat.Format32bppArgb);//设置长宽
            using (var g = Graphics.FromImage(bit))
            {
                Brush p0 = new SolidBrush(Color.White);
                g.FillRectangle(p0, 0, 0, 600, y);
                int padding_width = 100;
                int padding_height = 30;
                Pen pen = new Pen(Color.Black);
                for (int i = 0; i <= y / padding_height; i++)//画横线
                {
                    //画线的方法，第一个参数为起始点X的坐标，第二个参数为起始
                    //点Y的坐标；第三个参数为终点X的坐标，第四个参数为终
                    //点Y的坐标；
                    g.DrawLine(pen, 0, padding_height * i, 600, padding_height * i);
                }
                for (int i = 0; i <= 600 / padding_width; i++)//画竖线
                {
                    g.DrawLine(pen, padding_width * i, 0, padding_width * i, y);
                }
                Brush p1 = new SolidBrush(Color.Yellow);
                Brush p2 = new SolidBrush(Color.DarkGray);
                Brush p3 = new SolidBrush(Color.Gray);
                Brush p4 = new SolidBrush(Color.SlateBlue);
                Brush p5 = new SolidBrush(Color.Pink);
                g.FillRectangle(p1, 0, 0, 100, 30);
                g.FillRectangle(p2, 100, 0, 100, 30);
                g.FillRectangle(p3, 200, 0, 100, 30);
                g.FillRectangle(p4, 300, 0, 100, 30);
                g.FillRectangle(p5, 400, 0, 100, 30);
                g.FillRectangle(p2, 500, 0, 100, 30);
                g.FillRectangle(p1, 0, (count) * 30, 100, 30);
                g.FillRectangle(p2, 100, (count) * 30, 100, 30);
                g.FillRectangle(p3, 200, (count) * 30, 100, 30);
                g.FillRectangle(p4, 300, (count) * 30, 100, 30);
                g.FillRectangle(p5, 400, (count) * 30, 100, 30);
                g.FillRectangle(p2, 500, (count) * 30, 100, 30);
                StringFormat format = new StringFormat();
                format.LineAlignment = StringAlignment.Center;  // 更正： 垂直居中
                format.Alignment = StringAlignment.Center;
                for (int i = 0; i < count; i++)
                {
                    g.DrawString(dataGridView1[0, i].Value.ToString(), new Font(new FontFamily("宋体"), 15, FontStyle.Bold), Brushes.Black, new PointF(50, (i * 30) + 16), format);
                    g.DrawString(dataGridView1[1, i].Value.ToString(), new Font(new FontFamily("宋体"), 15, FontStyle.Bold), Brushes.Blue, new PointF(150, (i * 30) + 16), format);
                    g.DrawString(dataGridView1[2, i].Value.ToString(), new Font(new FontFamily("宋体"), 15, FontStyle.Bold), Brushes.Red, new PointF(250, (i * 30) + 16), format);
                    g.DrawString(dataGridView1[3, i].Value.ToString(), new Font(new FontFamily("宋体"), 15, FontStyle.Bold), Brushes.Black, new PointF(350, (i * 30) + 16), format);
                    g.DrawString(dataGridView1[4, i].Value.ToString(), new Font(new FontFamily("宋体"), 15, FontStyle.Bold), Brushes.Black, new PointF(450, (i * 30) + 16), format);
                    g.DrawString(dataGridView1[5, i].Value.ToString(), new Font(new FontFamily("宋体"), 15, FontStyle.Bold), Brushes.Lime, new PointF(550, (i * 30) + 16), format);
                }
                g.DrawString(dataGridView3[0, 0].Value.ToString(), new Font(new FontFamily("宋体"), 15, FontStyle.Bold), Brushes.Black, new PointF(50, (count * 30) + 16), format);
                g.DrawString(dataGridView3[1, 0].Value.ToString(), new Font(new FontFamily("宋体"), 15, FontStyle.Bold), Brushes.Blue, new PointF(150, (count * 30) + 16), format);
                g.DrawString(dataGridView3[2, 0].Value.ToString(), new Font(new FontFamily("宋体"), 15, FontStyle.Bold), Brushes.Red, new PointF(250, (count * 30) + 16), format);
                g.DrawString(dataGridView3[3, 0].Value.ToString(), new Font(new FontFamily("宋体"), 15, FontStyle.Bold), Brushes.Black, new PointF(350, (count * 30) + 16), format);
                g.DrawString(dataGridView3[4, 0].Value.ToString(), new Font(new FontFamily("宋体"), 15, FontStyle.Bold), Brushes.Black, new PointF(450, (count * 30) + 16), format);
                g.DrawString(dataGridView3[5, 0].Value.ToString(), new Font(new FontFamily("宋体"), 15, FontStyle.Bold), Brushes.Lime, new PointF(550, (count * 30) + 16), format);
                //这里绘图
                g.Flush();
            }
            bit.Save("0.png");
            WXService wxs = new WXService();
            wxs.Wx_SendFile(frmGroup.groupid, "0.png");
            wxs.Wx_SendFile_Gif(frmGroup.groupid, "emoji/confim1.gif");
            wxs.Wx_SendFile_Gif(frmGroup.groupid, "emoji/confim2.gif");
            wxs.Wx_SendFile_Gif(frmGroup.groupid, "emoji/open.gif");
            button11.Enabled = false ;
            button12.Enabled = true;

            //计算压法
            int x=0, z = 0, xd = 0, zd = 0, h = 0;
            float yx = 0, yz = 0, yxd = 0, yzd = 0, yh = 0;//系统推荐分
            int yx1, yz1, yxd1, yzd1, yh1;//真实压分

            for (int i = 1; i < dataGridView1.Rows.Count; i++)
            {
                SqlConnection myConn = new SqlConnection(connstr);//创建数据库连接类的对象
                myConn.Open(); //将连接打开
                SqlDataAdapter myDataAdapter = new SqlDataAdapter("select * from shishishuju where nickname= '" + dataGridView1[0,i].Value  + "'", myConn);
                DataSet myDataSet = new DataSet();
                myDataAdapter.Fill(myDataSet, "shishishuju");
                DataTable myTable = myDataSet.Tables["shishishuju"];
                foreach (DataRow myRow in myTable.Rows)
                {
                    if (Convert.ToInt32(myRow["flag"]) > 0)
                    {
                        x = x +Convert.ToInt32(dataGridView1[1, i].Value);
                        z = z +Convert.ToInt32(dataGridView1[2, i].Value);
                        xd =xd+Convert.ToInt32(dataGridView1[3, i].Value);
                        zd =zd+Convert.ToInt32(dataGridView1[4, i].Value);
                        h = h +Convert.ToInt32(dataGridView1[5, i].Value);
                    }
                }
                myDataSet.Clear();
                myTable.Clear();
                myConn.Close();

            }

            




           // x = Convert.ToInt32(dataGridView3[1,0].Value );
           // z = Convert.ToInt32(dataGridView3[2, 0].Value);
           // xd =Convert.ToInt32(dataGridView3[3, 0].Value);
           // zd =Convert.ToInt32(dataGridView3[4, 0].Value);
           // h = Convert.ToInt32(dataGridView3[5,0].Value);
            if ( Math.Abs(x - z) >= taifei)
            {
                if (z > x)
                {
                    yz =  (z - x);
                    yx = 0;
                    yxd = xd;
                    yzd = zd;
                    yh =  h;
                }
                if (x > z)
                {
                    yx = (x - z);
                    yz = 0;
                    yxd =  xd;
                    yzd =  zd;
                    yh =   h;
                }
            }
            else
            {
                if (z > x)
                {
                    yz = (z - x) + taifei;
                    yx = taifei;
                    yxd =  xd;
                    yzd =  zd;
                    yh =   h;
                }
                if (x > z)
                {
                    yx = (x - z) + taifei;
                    yz = taifei;
                    yxd =  xd;
                    yzd =  zd;
                    yh =   h;
                }
                if (x == z)
                {
                    yx = 0;
                    yz = 0;
                    yxd =  xd;
                    yzd =  zd;
                    yh =   h;
                }
            }
            yx1 = Convert.ToInt32(Round(yx / 100, 1)) * 100;
            yz1 = Convert.ToInt32(Round(yz / 100, 1)) * 100;
            yxd1 = Convert.ToInt32(Round(yxd / 100, 1)) * 100;
            yzd1 = Convert.ToInt32(Round(yzd / 100, 1)) * 100;
            yh1 = Convert.ToInt32(Round(yh / 100, 1)) * 100;


            textBox3.Text = yx.ToString();
            textBox4.Text = yz.ToString();
            textBox5.Text = yxd.ToString();
            textBox6.Text = yzd.ToString();
            textBox7.Text = yh.ToString();


            textBox12.Text = yx1.ToString();
            textBox11.Text = yz1.ToString();
            textBox10.Text = "0";
            textBox9.Text = "0";
            textBox8.Text = "0";
            button21.Text = "补数";
        }
        private void button14_Click(object sender, EventArgs e)
        {
            int count = dataGridView1.RowCount;
            int y = (count+1) * 30;
            Bitmap bit = new Bitmap(400, y, PixelFormat.Format32bppArgb);//设置长宽
            using (var g = Graphics.FromImage(bit))
            {
                Brush p0 = new SolidBrush(Color.White);
                g.FillRectangle(p0, 0, 0, 400, y);
                int padding_width = 100;
                int padding_height = 30;
                Pen pen = new Pen(Color.Black);
                for (int i = 0; i <= y / padding_height; i++)//画横线
                {
                    //画线的方法，第一个参数为起始点X的坐标，第二个参数为起始
                    //点Y的坐标；第三个参数为终点X的坐标，第四个参数为终
                    //点Y的坐标；
                    g.DrawLine(pen, 0, padding_height * i, 600, padding_height * i);
                }
                for (int i = 0; i <= 600 / padding_width; i++)//画竖线
                {
                    g.DrawLine(pen, padding_width * i, 0, padding_width * i, y);
                }
                Brush p1 = new SolidBrush(Color.Yellow);
                Brush p2 = new SolidBrush(Color.DarkGray);
                Brush p3 = new SolidBrush(Color.Gray);
                Brush p4 = new SolidBrush(Color.SlateBlue);
                Brush p5 = new SolidBrush(Color.Pink);
                g.FillRectangle (p1, 0,0,100,30);
                g.FillRectangle (p2, 100, 0, 100, 30);
                g.FillRectangle (p3, 200, 0, 100, 30);
                g.FillRectangle (p4, 300, 0, 100, 30);
                g.FillRectangle(p1, 0,   (count)*30, 100, 30);
                g.FillRectangle(p2, 100, (count)*30, 100, 30);
                g.FillRectangle(p3, 200, (count)*30, 100, 30);
                g.FillRectangle(p4, 300, (count)*30, 100, 30);
                StringFormat format = new StringFormat();
                format.LineAlignment = StringAlignment.Center;  // 更正： 垂直居中
                format.Alignment = StringAlignment.Center;
                for (int i=0;i<count;i++)
                {
                    g.DrawString(dataGridView2[0, i].Value.ToString(), new Font(new FontFamily("宋体"), 15, FontStyle.Bold), Brushes.Black, new PointF(50, (i * 30) + 16), format);
                    g.DrawString(dataGridView2[1, i].Value.ToString(), new Font(new FontFamily("宋体"), 15, FontStyle.Bold), Brushes.Blue, new PointF(150, (i * 30) + 16), format);
                    g.DrawString(dataGridView2[2, i].Value.ToString(), new Font(new FontFamily("宋体"), 15, FontStyle.Bold), Brushes.Red, new PointF(250, (i * 30) + 16), format);
                    g.DrawString(dataGridView2[3, i].Value.ToString(), new Font(new FontFamily("宋体"), 15, FontStyle.Bold), Brushes.Black, new PointF(350, (i * 30) + 16), format);
                }
                g.DrawString(dataGridView4[0, 0].Value.ToString(), new Font(new FontFamily("宋体"), 15, FontStyle.Bold), Brushes.Black, new PointF(50, (count * 30) + 16), format);
                g.DrawString(dataGridView4[1, 0].Value.ToString(), new Font(new FontFamily("宋体"), 15, FontStyle.Bold), Brushes.Blue, new PointF(150, (count * 30) + 16), format);
                g.DrawString(dataGridView4[2, 0].Value.ToString(), new Font(new FontFamily("宋体"), 15, FontStyle.Bold), Brushes.Red, new PointF(250,  (count * 30) + 16), format);
                g.DrawString(dataGridView4[3, 0].Value.ToString(), new Font(new FontFamily("宋体"), 15, FontStyle.Bold), Brushes.Black, new PointF(350,(count * 30) + 16), format);
                //这里绘图
                g.Flush();
            }
            bit.Save("1.png");
            WXService wxs = new WXService();
            wxs.Wx_SendFile(frmGroup .groupid ,"1.png");
            button14.Enabled = false;
        }
        private void button13_Click(object sender, EventArgs e)
        {
            Image img;
            IDataObject iData = Clipboard.GetDataObject();
            if (iData.GetDataPresent(DataFormats.Bitmap))
            {
                img = System.Windows.Forms.Clipboard.GetImage();
                img.Save("3.png");
                WXService wxs = new WXService();
                wxs.Wx_SendFile(frmGroup.groupid, "3.png");
                Clipboard.Clear();
                button13.Enabled = false;
                button14.Enabled = true;
            }
            else
            {
                MessageBox.Show("请先在路单系统中截图！截图之后在发送路单之前不要进行复制操作！！！");
            }
        }
        private void button17_Click(object sender, EventArgs e)
        {
            for (int i = 1; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1[1, i].Value = 0;
                dataGridView1[2, i].Value = 0;
                dataGridView1[3, i].Value = 0;
                dataGridView1[4, i].Value = 0;
                dataGridView1[5, i].Value = 0;
            }
            dataGridView3[1, 0].Value = 0;
            dataGridView3[2, 0].Value = 0;
            dataGridView3[3, 0].Value = 0;
            dataGridView3[4, 0].Value = 0;
            dataGridView3[5, 0].Value = 0;
        }
        private void button18_Click(object sender, EventArgs e)
        {
            WXService wxs = new WXService();
            wxs.Wx_SendFile_Gif(frmGroup.groupid, "emoji/last10.gif");
            button10.Enabled = true;
        }
        private void button12_Click(object sender, EventArgs e)
        {
            if ((radioButton1.Checked==false )&& (radioButton2.Checked == false) && (radioButton3.Checked == false) && (checkBox1.Checked == false) && (checkBox2.Checked == false) )
            {
                MessageBox.Show("请先开奖！");
                return;
            }
            for (int i = 1; i < dataGridView1.Rows.Count; i++)
            {
                double  sum = 0;
                if (radioButton1.Checked == true)//庄---1.95倍
                {
                    sum = sum +(Convert.ToInt32(dataGridView1[2, i].Value) * 0.95);
                }
                else 
                {
                    sum = sum - Convert.ToInt32(dataGridView1[2, i].Value);
                }
                if (radioButton2.Checked == true)//闲---2倍
                {
                    sum = sum + Convert.ToInt32(dataGridView1[1, i].Value);
                }
                else
                {
                    sum = sum - Convert.ToInt32(dataGridView1[1, i].Value);
                }
                if (radioButton3.Checked == true)//和---9倍
                {
                    sum = sum + (Convert.ToInt32(dataGridView1[5, i].Value) * 8);
                }
                else
                {
                    sum = sum - Convert.ToInt32(dataGridView1[5, i].Value);
                }
                if (checkBox1.Checked == true)//庄对---12倍
                {
                    sum = sum + (Convert.ToInt32(dataGridView1[4, i].Value) * 11);
                }
                else
                {
                    sum = sum - Convert.ToInt32(dataGridView1[4, i].Value);
                }
                if (checkBox2.Checked == true)//闲对---12倍
                {
                    sum = sum + (Convert.ToInt32(dataGridView1[3, i].Value) * 11);
                }
                else
                {
                    sum = sum - Convert.ToInt32(dataGridView1[3, i].Value);
                }
                dataGridView2[1, i].Value = sum;
                dataGridView2[2, i].Value =Convert .ToInt32 (dataGridView2[2, i].Value)+ sum;
                dataGridView4[1, 0].Value =Convert. ToInt32 (dataGridView4[1, 0].Value)+ sum ;
                dataGridView4[2, 0].Value =Convert. ToInt32(dataGridView4[2, 0].Value) + sum;
            }
            button12.Enabled = false;
            button13.Enabled = true;
        }
        private void button19_Click(object sender, EventArgs e)
        {
            button10.Enabled = true;
            button11.Enabled = true;
            button12.Enabled = true;
            button13.Enabled = true;
            button14.Enabled = true;
            button15.Enabled = true;
            button16.Enabled = true;
            button17.Enabled = true;
            button18.Enabled = true;
        }
        private void button20_Click(object sender, EventArgs e)
        {
            for (int i = 1; i < dataGridView2.Rows.Count; i++)
            {
                dataGridView4[1, 0].Value = Convert.ToInt32(dataGridView4[1, 0].Value) - Convert.ToInt32(dataGridView2[1, i].Value);
                dataGridView4[2, 0].Value = Convert.ToInt32(dataGridView4[2, 0].Value) - Convert.ToInt32(dataGridView2[1, i].Value);
                dataGridView2[2, i].Value = Convert.ToInt32(dataGridView2[2, i].Value) - Convert.ToInt32(dataGridView2[1, i].Value);
                dataGridView2[1, i].Value = 0;
            }
        }
        private void button21_Click(object sender, EventArgs e)
        {
            if (button21.Text == "补数")
            {
                if ((Convert.ToInt32(Round(Convert.ToInt32(textBox3.Text) / 100, 1)) * 100 >= Convert.ToInt32(taifei)) && ((Convert.ToInt32(Round(Convert.ToInt32(textBox3.Text) / 100, 1)) * 100) - (Convert.ToInt32(Round(Convert.ToInt32(textBox4.Text) / 100, 1)) * 100) >= Convert.ToInt32(taifei) / 2))
                {
                    textBox12.Text = taifei.ToString ();
                    textBox11.Text = "0";
                }
                if ((Convert.ToInt32(Round(Convert.ToInt32(textBox4.Text) / 100, 1)) * 100 >= Convert.ToInt32(taifei)) && ((Convert.ToInt32(Round(Convert.ToInt32(textBox4.Text) / 100, 1)) * 100) - (Convert.ToInt32(Round(Convert.ToInt32(textBox3.Text) / 100, 1)) * 100) >= Convert.ToInt32(taifei) / 2))
                {
                    textBox12.Text = "0";
                    textBox11.Text = taifei.ToString();
                    
                }
                if ((Convert.ToInt32(Round(Convert.ToInt32(textBox3.Text) / 100, 1)) * 100 == Convert.ToInt32(taifei)) && ((Convert.ToInt32(Round(Convert.ToInt32(textBox4.Text) / 100, 1)) * 100) - (Convert.ToInt32(Round(Convert.ToInt32(textBox3.Text) / 100, 1)) * 100) < Convert.ToInt32(taifei) / 2))
                {
                    textBox12.Text = "0";
                    textBox11.Text = "0";
                }
                if ((Convert.ToInt32(Round(Convert.ToInt32(textBox4.Text) / 100, 1)) * 100 == Convert.ToInt32(taifei)) && ((Convert.ToInt32(Round(Convert.ToInt32(textBox3.Text) / 100, 1)) * 100) - (Convert.ToInt32(Round(Convert.ToInt32(textBox4.Text) / 100, 1)) * 100) < Convert.ToInt32(taifei) / 2))
                {
                    textBox12.Text = "0";
                    textBox11.Text = "0";
                    
                }
                button21.Text = "不补数";
            }
            else
            {
                textBox12.Text = (Convert.ToInt32(Round(Convert.ToInt32(textBox3.Text) / 100, 1)) * 100).ToString();
                textBox11.Text = (Convert.ToInt32(Round(Convert.ToInt32(textBox4.Text) / 100, 1)) * 100).ToString();
                button21.Text = "补数";
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            label20.Text = "0";
            label21.Text = "0";
            label22.Text = "0";
            label24.Text = "0";
            label25.Text = "0";
            SqlConnection myConn = new SqlConnection(connstr);//创建数据库连接类的对象
            myConn.Open(); //将连接打开
            SqlDataAdapter myDataAdapter = new SqlDataAdapter("select * from youxijilu where timestamp = '" + frmGroup.timestamp + "'order by xue desc", myConn);
            DataSet myDataSet = new DataSet();
            myDataAdapter.Fill(myDataSet, "youxijilu");
            DataTable myTable = myDataSet.Tables["youxijilu"];
            foreach (DataRow myRow in myTable.Rows)
            {
                label20.Text = (Convert.ToInt32(label20.Text) + Convert.ToInt32(myRow["choumayinli"])).ToString ();
                label21.Text = (Convert.ToInt32(label21.Text) + Convert.ToInt32(myRow["fenshuyinli"])).ToString();
                label22.Text = (Convert.ToInt32(label22.Text) + Convert.ToInt32(myRow["ximayinli"])).ToString();
                label24.Text = (Convert.ToInt32(label24.Text) + Convert.ToInt32(myRow["sanbaoyinli"])).ToString();
                label25.Text = (Convert.ToInt32(label25.Text) + Convert.ToInt32(myRow["duichongyinli"])).ToString();
            }
            myDataSet.Clear();
            myTable.Clear();
            myConn.Close();

            wxcaozuo wxcz = new wxcaozuo();
            textBox13.Text = wxcz.IniReadValue("系统设置", "格式错误", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
            textBox14.Text= wxcz.IniReadValue("系统设置", "余额不足", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
            textBox15.Text = wxcz.IniReadValue("系统设置", "未到下注时间", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
            textBox19.Text = wxcz.IniReadValue("系统设置", "添加成员", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
            
                textBox23.Text = wxcz.IniReadValue("下注格式", "庄", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
                textBox22.Text = wxcz.IniReadValue("下注格式", "闲", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
                textBox21.Text = wxcz.IniReadValue("下注格式", "庄对", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
                textBox20.Text = wxcz.IniReadValue("下注格式", "闲对", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
                textBox24.Text = wxcz.IniReadValue("下注格式", "和", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
           
            textBox25.Text = wxcz.IniReadValue("系统设置", "台费", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");

        }
        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        private void button15_Click(object sender, EventArgs e)
        {
            if ((radioButton1.Checked == false) && (radioButton2.Checked == false) && (radioButton3.Checked == false) && (checkBox1.Checked == false) && (checkBox2.Checked == false))
            {
                MessageBox.Show("请先开奖！");
                return;
            }
            string jieguo = "";
            int sb = 0;//三宝盈利
            int dc = 0;//对冲盈利
            int sumfs = 0;//分数盈亏 
            int sumfs1 = 0;//分数盈亏 
            int sum = 0;//澳门盈亏
            int sum1 = 0;
            SqlConnection myConn = new SqlConnection(connstr);//创建数据库连接类的对象
            myConn.Open(); //将连接打开
            SqlDataAdapter myDataAdapter = new SqlDataAdapter("select * from youxijilu where timestamp = '" + frmGroup.timestamp + "'order by xue desc", myConn);
            DataSet myDataSet = new DataSet();
            myDataAdapter.Fill(myDataSet, "youxijilu");
            DataTable myTable = myDataSet.Tables["youxijilu"];
            DataRow myRow = myTable.NewRow();
            myRow["timestamp"] = frmGroup.timestamp;
            myRow["kou"] = dataGridView1[0, 0].Value.ToString().Substring(dataGridView1[0, 0].Value.ToString().IndexOf("靴") + 1, dataGridView1[0, 0].Value.ToString().IndexOf("局") - dataGridView1[0, 0].Value.ToString().IndexOf("靴") - 1);
            myRow["xue"] = dataGridView1[0, 0].Value.ToString().Substring(0, dataGridView1[0, 0].Value.ToString().IndexOf("靴"));
            ///微信下注结果
            myRow["x"] = dataGridView3[1, 0].Value.ToString();
            myRow["z"] = dataGridView3[2, 0].Value.ToString();
            myRow["xd"] = dataGridView3[3, 0].Value.ToString();
            myRow["zd"] = dataGridView3[4, 0].Value.ToString();
            myRow["h"] = dataGridView3[5, 0].Value.ToString();
            ///系统推荐压法
            myRow["yx"] = textBox3.Text;
            myRow["yz"] = textBox4.Text;
            myRow["yxd"] = textBox5.Text;
            myRow["yzd"] = textBox6.Text;
            myRow["yh"] = textBox7.Text;
            ///真实压法
            myRow["yx1"] =  textBox12.Text;
            myRow["yz1"] =  textBox11.Text;
            myRow["yxd1"] = textBox10.Text;
            myRow["yzd1"] = textBox9.Text;
            myRow["yh1"] =  textBox8.Text;
            //开奖结果
            if (radioButton1.Checked == true)
            {
                sumfs = Convert.ToInt32(Convert.ToInt32(textBox3.Text) * 2);
                sum = Convert.ToInt32(Convert.ToInt32(textBox12.Text) * 2);
                jieguo = jieguo + "&闲";
            }
            if (radioButton2.Checked == true)
            {
                sumfs = sumfs + Convert.ToInt32(Convert.ToInt32(textBox4.Text) * 1.95);
                sum = sum + Convert.ToInt32(Convert.ToInt32(textBox11.Text) * 1.95);
                jieguo = jieguo + "&庄";
            }
            if (radioButton3.Checked == true)
            {
                sumfs = sumfs + Convert.ToInt32(Convert.ToInt32(textBox7.Text) * 9);
                sum = sum + Convert.ToInt32(Convert.ToInt32(textBox8.Text) * 9);
                jieguo = jieguo + "&和";
            }
            if (checkBox1 .Checked == true)
            {
                sumfs = sumfs + Convert.ToInt32(Convert.ToInt32(textBox6.Text) * 12);
                sum = sum + Convert.ToInt32(Convert.ToInt32(textBox9.Text) * 12);
                jieguo = jieguo + "&庄对";
            }
            if (checkBox2.Checked == true)
            {
                sumfs = sumfs + Convert.ToInt32(Convert.ToInt32(textBox5.Text) * 12);
                sum = sum + Convert.ToInt32(Convert.ToInt32(textBox10.Text) * 12);
                jieguo = jieguo + "&闲对";
            }
            if (radioButton3 .Checked == true)
            {
                sumfs1 = sumfs - Convert.ToInt32(textBox5.Text) - Convert.ToInt32(textBox6.Text) - Convert.ToInt32(textBox7.Text);
                sum1 = sum - Convert.ToInt32(textBox10.Text) - Convert.ToInt32(textBox9.Text) - Convert.ToInt32(textBox8.Text);
            }
            else
            {
                sumfs1 = sumfs - Convert.ToInt32(textBox3.Text) - Convert.ToInt32(textBox4.Text) - Convert.ToInt32(textBox5.Text) - Convert.ToInt32(textBox6.Text) - Convert.ToInt32(textBox7.Text);
                sum1 = sum - Convert.ToInt32(textBox12.Text) - Convert.ToInt32(textBox11.Text) - Convert.ToInt32(textBox10.Text) - Convert.ToInt32(textBox9.Text) - Convert.ToInt32(textBox8.Text);
            }
            myRow["choumayinli"] = sum1;
            myRow["fenshuyinli"] = sumfs1;
            if (jieguo.Substring(0, 1) == "&")
            {
                jieguo = jieguo.Remove(0, 1);
            }
            myRow["jieguo"] = jieguo;
            //三宝盈利
            if (checkBox1.Checked == true)
            {
                sb = sb - (Convert.ToInt32(textBox5.Text) - Convert.ToInt32(textBox10.Text)) * 12;
            }
            if (checkBox2.Checked == true)
            {
                sb = sb - (Convert.ToInt32(textBox6.Text) - Convert.ToInt32(textBox9.Text)) * 12;
            }
            if (radioButton3.Checked == true)
            {
                sb = sb - (Convert.ToInt32(textBox7.Text) - Convert.ToInt32(textBox8.Text)) * 9;
            }
            myRow["sanbaoyinli"] = sb;
            //对冲盈利
            if (radioButton1 .Checked == true)
            {
                if (Convert.ToInt32(textBox3.Text) > Convert.ToInt32(textBox4.Text))
                {
                    dc = Convert.ToInt32(Convert.ToInt32(textBox4.Text) * 0.05);
                }
                else
                {
                    if ((textBox11.Text == "0") && (textBox12.Text == "0"))
                    {
                        dc = Convert.ToInt32(Convert.ToInt32(textBox4.Text) * 0.05);
                    }
                    else
                    {
                        dc = Convert.ToInt32(Convert.ToInt32(textBox3.Text) * 0.05);
                    }
                }
            }
            myRow["duichongyinli"] = dc;
            //洗码盈利
            int xima = 0;
            if (radioButton1 .Checked == false)
            {
                xima = Convert.ToInt32(textBox11.Text);
            }
            if (radioButton2.Checked == false)
            {
                xima = Convert.ToInt32(textBox12.Text );
            }
            myRow["ximayinli"] = xima;

            dataGridView1[0, 0].Value = dataGridView1[0, 0].Value.ToString().Substring(0, dataGridView1[0, 0].Value.ToString().IndexOf("靴"))+"靴"+ (Convert .ToInt32 (dataGridView1[0, 0].Value.ToString().Substring(dataGridView1[0, 0].Value.ToString().IndexOf("靴") + 1, dataGridView1[0, 0].Value.ToString().IndexOf("局") - dataGridView1[0, 0].Value.ToString().IndexOf("靴") - 1))+1)+"局";
            dataGridView2[0, 0].Value = dataGridView1[0, 0].Value;
            myTable.Rows.Add(myRow);
            SqlCommandBuilder mySqlCommandBuilder = new SqlCommandBuilder(myDataAdapter);
            myDataAdapter.Update(myDataSet, "youxijilu");
            myConn.Close();


            radioButton1.Checked = false;
            radioButton2.Checked = false;
            radioButton3.Checked = false;
            checkBox1.Checked = false;
            checkBox2.Checked = false;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            if ((radioButton1.Checked == false) && (radioButton2.Checked == false) && (radioButton3.Checked == false) && (checkBox1.Checked == false) && (checkBox2.Checked == false))
            {
                MessageBox.Show("请先开奖！");
                return;
            }
            string jieguo = "";
            int sb = 0;//三宝盈利
            int dc = 0;//对冲盈利
            int sumfs = 0;//分数盈亏 
            int sumfs1 = 0;//分数盈亏 
            int sum = 0;//澳门盈亏
            int sum1 = 0;
            SqlConnection myConn = new SqlConnection(connstr);//创建数据库连接类的对象
            myConn.Open(); //将连接打开
            SqlDataAdapter myDataAdapter = new SqlDataAdapter("select * from youxijilu where timestamp = '" + frmGroup.timestamp + "'order by xue desc", myConn);
            DataSet myDataSet = new DataSet();
            myDataAdapter.Fill(myDataSet, "youxijilu");
            DataTable myTable = myDataSet.Tables["youxijilu"];
            DataRow myRow = myTable.NewRow();
            myRow["timestamp"] = frmGroup.timestamp;
            myRow["kou"] = dataGridView1[0, 0].Value.ToString().Substring(dataGridView1[0, 0].Value.ToString().IndexOf("靴") + 1, dataGridView1[0, 0].Value.ToString().IndexOf("局") - dataGridView1[0, 0].Value.ToString().IndexOf("靴") - 1);
            myRow["xue"] = dataGridView1[0, 0].Value.ToString().Substring(0, dataGridView1[0, 0].Value.ToString().IndexOf("靴"));
            ///微信下注结果
            myRow["x"] = dataGridView3[1, 0].Value.ToString();
            myRow["z"] = dataGridView3[2, 0].Value.ToString();
            myRow["xd"] = dataGridView3[3, 0].Value.ToString();
            myRow["zd"] = dataGridView3[4, 0].Value.ToString();
            myRow["h"] = dataGridView3[5, 0].Value.ToString();
            ///系统推荐压法
            myRow["yx"] = textBox3.Text;
            myRow["yz"] = textBox4.Text;
            myRow["yxd"] = textBox5.Text;
            myRow["yzd"] = textBox6.Text;
            myRow["yh"] = textBox7.Text;
            ///真实压法
            myRow["yx1"] = textBox12.Text;
            myRow["yz1"] = textBox11.Text;
            myRow["yxd1"] = textBox10.Text;
            myRow["yzd1"] = textBox9.Text;
            myRow["yh1"] = textBox8.Text;
            //开奖结果
            if (radioButton1.Checked == true)
            {
                sumfs = Convert.ToInt32(Convert.ToInt32(textBox3.Text) * 2);
                sum = Convert.ToInt32(Convert.ToInt32(textBox12.Text) * 2);
                jieguo = jieguo + "&闲";
            }
            if (radioButton2.Checked == true)
            {
                sumfs = sumfs + Convert.ToInt32(Convert.ToInt32(textBox4.Text) * 1.95);
                sum = sum + Convert.ToInt32(Convert.ToInt32(textBox11.Text) * 1.95);
                jieguo = jieguo + "&庄";
            }
            if (radioButton3.Checked == true)
            {
                sumfs = sumfs + Convert.ToInt32(Convert.ToInt32(textBox7.Text) * 9);
                sum = sum + Convert.ToInt32(Convert.ToInt32(textBox8.Text) * 9);
                jieguo = jieguo + "&和";
            }
            if (checkBox1.Checked == true)
            {
                sumfs = sumfs + Convert.ToInt32(Convert.ToInt32(textBox6.Text) * 12);
                sum = sum + Convert.ToInt32(Convert.ToInt32(textBox9.Text) * 12);
                jieguo = jieguo + "&庄对";
            }
            if (checkBox2.Checked == true)
            {
                sumfs = sumfs + Convert.ToInt32(Convert.ToInt32(textBox5.Text) * 12);
                sum = sum + Convert.ToInt32(Convert.ToInt32(textBox10.Text) * 12);
                jieguo = jieguo + "&闲对";
            }
            if (radioButton3.Checked == true)
            {
                sumfs1 = sumfs - Convert.ToInt32(textBox5.Text) - Convert.ToInt32(textBox6.Text) - Convert.ToInt32(textBox7.Text);
                sum1 = sum - Convert.ToInt32(textBox10.Text) - Convert.ToInt32(textBox9.Text) - Convert.ToInt32(textBox8.Text);
            }
            else
            {
                sumfs1 = sumfs - Convert.ToInt32(textBox3.Text) - Convert.ToInt32(textBox4.Text) - Convert.ToInt32(textBox5.Text) - Convert.ToInt32(textBox6.Text) - Convert.ToInt32(textBox7.Text);
                sum1 = sum - Convert.ToInt32(textBox12.Text) - Convert.ToInt32(textBox11.Text) - Convert.ToInt32(textBox10.Text) - Convert.ToInt32(textBox9.Text) - Convert.ToInt32(textBox8.Text);
            }
            myRow["choumayinli"] = sum1;
            myRow["fenshuyinli"] = sumfs1;
            if (jieguo.Substring(0, 1) == "&")
            {
                jieguo = jieguo.Remove(0, 1);
            }
            myRow["jieguo"] = jieguo;
            //三宝盈利
            if (checkBox1.Checked == true)
            {
                sb = sb - (Convert.ToInt32(textBox5.Text) - Convert.ToInt32(textBox10.Text)) * 12;
            }
            if (checkBox2.Checked == true)
            {
                sb = sb - (Convert.ToInt32(textBox6.Text) - Convert.ToInt32(textBox9.Text)) * 12;
            }
            if (radioButton3.Checked == true)
            {
                sb = sb - (Convert.ToInt32(textBox7.Text) - Convert.ToInt32(textBox8.Text)) * 9;
            }
            myRow["sanbaoyinli"] = sb;
            //对冲盈利
            if (radioButton1.Checked == true)
            {
                if (Convert.ToInt32(textBox3.Text) > Convert.ToInt32(textBox4.Text))
                {
                    dc = Convert.ToInt32(Convert.ToInt32(textBox4.Text) * 0.05);
                }
                else
                {
                    if ((textBox11.Text == "0") && (textBox12.Text == "0"))
                    {
                        dc = Convert.ToInt32(Convert.ToInt32(textBox4.Text) * 0.05);
                    }
                    else
                    {
                        dc = Convert.ToInt32(Convert.ToInt32(textBox3.Text) * 0.05);
                    }
                }
            }
            myRow["duichongyinli"] = dc;
            //洗码盈利
            int xima = 0;
            if (radioButton1.Checked == false)
            {
                xima = Convert.ToInt32(textBox11.Text);
            }
            if (radioButton2.Checked == false)
            {
                xima = Convert.ToInt32(textBox12.Text);
            }
            myRow["ximayinli"] = xima;

            dataGridView1[0, 0].Value = (Convert .ToInt32( myRow["xue"] )+1)+ "靴" + "1局";
            dataGridView2[0, 0].Value = dataGridView1[0, 0].Value;
            myTable.Rows.Add(myRow);
            SqlCommandBuilder mySqlCommandBuilder = new SqlCommandBuilder(myDataAdapter);
            myDataAdapter.Update(myDataSet, "youxijilu");
            myConn.Close();


            radioButton1.Checked = false;
            radioButton2.Checked = false;
            radioButton3.Checked = false;
            checkBox1.Checked = false;
            checkBox2.Checked = false;
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void button22_Click(object sender, EventArgs e)
        {

        }

        private void button23_Click(object sender, EventArgs e)
        {
            WXService wxs = new HTTP.WXService();
            wxcaozuo wxcz = new wxcaozuo();
            if (wxcz.IniReadValue("系统设置", "格式错误", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini") != "")
            {
                wxs.SendMsg("@" + wxcz.IniReadValue("系统设置", "格式错误", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini"), usernameid, frmGroup.groupid, 1);
            }

            string ssss= wxcz. IniReadValue("系统设置", "格式错误", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
            wxs.SendMsg( ssss, usernameid, frmGroup.groupid, 1);
        }

        private void button25_Click(object sender, EventArgs e)
        {
            if ((textBox23.Text.Substring(textBox23.Text.Length - 1, 1) == "|") && (textBox22.Text.Substring(textBox22.Text.Length - 1, 1) == "|") && (textBox21.Text.Substring(textBox21.Text.Length - 1, 1) == "|") && (textBox20.Text.Substring(textBox20.Text.Length - 1, 1) == "|") && (textBox24.Text.Substring(textBox24.Text.Length - 1, 1) == "|"))
            {
                wxcaozuo wxcz = new caozuo.wxcaozuo();
                wxcz.IniWrite("下注格式", "庄", textBox23.Text, System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
                wxcz.IniWrite("下注格式", "闲", textBox22.Text, System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
                wxcz.IniWrite("下注格式", "庄对", textBox21.Text, System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
                wxcz.IniWrite("下注格式", "闲对", textBox20.Text, System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
                wxcz.IniWrite("下注格式", "和", textBox24.Text, System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
                wxcz.IniWrite("系统设置", "格式错误", textBox13.Text, System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
                wxcz.IniWrite("系统设置", "余额不足", textBox14.Text, System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
                wxcz.IniWrite("系统设置", "未到下注时间", textBox15.Text, System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
                wxcz.IniWrite("系统设置", "添加成员", textBox19.Text, System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
                wxcz.IniWrite("系统设置", "台费", textBox25.Text, System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
            }
            else { MessageBox.Show("下注格式编辑错误！"); }
        }
    }
}

