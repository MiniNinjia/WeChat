using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using WeChat.NET.Controls;
using WeChat.NET.Objects;
using WeChat.NET.HTTP;
using Newtonsoft.Json.Linq;
using WeChat.NET.caozuo;
using System.Data.SqlClient;

namespace WeChat.NET
{
    public partial class frmGroup : Form
    {
        public frmGroup()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        public string connstr = "Data Source =.; Integrated Security = SSPI; Initial Catalog = database";
        public static string groupid = "";
        public static long  timestamp;
        private void frmGroup_Load(object sender, EventArgs e)
        {
                WXService wxs = new WXService();
               // JObject init_result = wxs.WxInit();//初始化
                JObject contact_result = wxs.GetContact(); //通讯录
            
            if (contact_result != null)
                {
                   this.listView1.BeginUpdate();   //数据更新，UI暂时挂起，直到EndUpdate绘制控件，可以有效避免闪烁并大大提高加载速度  
                    foreach (JObject contact in contact_result["MemberList"])  //完整好友名单
                    {
                        WXUser user = new WXUser();
                        if (contact["UserName"].ToString().IndexOf("@@") >= 0)
                        {
                            ListViewItem lvi = new ListViewItem();
                            lvi.Text = contact["NickName"].ToString();
                            lvi.SubItems.Add(contact["UserName"].ToString());
                            this.listView1.Items.Add(lvi);
                        }
                    }
                   this.listView1.EndUpdate();  //结束数据处理，UI界面一次性绘制。 
                }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            frmGroup_Load( sender,  e);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

            
            if (groupid=="")
            {
                return;
            }
            listView2.Items.Clear();
            WXService wxs = new WXService();
            wxcaozuo wxcz = new wxcaozuo();
            wxcz.Delete();
            JObject contact_result = wxs.GetGroupItem(groupid); //群聊
            if (contact_result != null)
            {
                this.listView2.BeginUpdate();   //数据更新，UI暂时挂起，直到EndUpdate绘制控件，可以有效避免闪烁并大大提高加载速度  
                foreach (JObject contactlist in contact_result["ContactList"])  //完整好友名单
                {
                     foreach (JObject contact in contactlist["MemberList"])  //完整好友名单
                     {
                             WXUser user = new WXUser();
                             ListViewItem lvi = new ListViewItem();
                             lvi.Text = contact["NickName"].ToString();
                             this.listView2.Items.Add(lvi);
                             wxcz.Write("<NickName>"+ contact["NickName"].ToString() + "<UserName>"+ contact["UserName"].ToString() + Environment.NewLine, "Append");
                     }
                }
                this.listView2.EndUpdate();  //结束数据处理，UI界面一次性绘制。 
            }
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
           
        }

        private void listView1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in listView1.SelectedItems)  //选中项遍历 
            {
                groupid = lvi.SubItems[1].Text;
                label2 .Text = lvi.SubItems[0].Text;
            }
        }
        private void groupBox1_Enter(object sender, EventArgs e)
        {
        }
        public static int flagchecked ;//（1为继续上次报表，0为不继续）
        private void button3_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                SqlConnection myConn = new SqlConnection(connstr);//创建数据库连接类的对象
                myConn.Open(); //将连接打开
                SqlDataAdapter myDataAdapter = new SqlDataAdapter("select max(timestamp) from timestamp ", myConn);
                DataSet myDataSet = new DataSet();
                myDataAdapter.Fill(myDataSet, "timestamp");
                DataTable myTable = myDataSet.Tables["timestamp"];
                    foreach (DataRow myRow in myTable.Rows)
                    {
                        timestamp =Convert .ToInt32 (myRow[0].ToString ());
                    }
                    myConn.Close();
                flagchecked = 1;
            }
            else
            {
                SqlConnection myConn = new SqlConnection(connstr);//创建数据库连接类的对象
                myConn.Open(); //将连接打开
                SqlDataAdapter myDataAdapter = new SqlDataAdapter("select * from timestamp ", myConn);
                DataSet myDataSet = new DataSet();
                myDataAdapter.Fill(myDataSet, "timestamp");
                DataTable myTable = myDataSet.Tables["timestamp"];
                DataRow myRow = myTable.NewRow();
                timestamp =  (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
                myRow["timestamp"] = timestamp;
                myRow["time"] = DateTime.Now.ToString("F");
                myTable.Rows.Add(myRow);
                SqlCommandBuilder mySqlCommandBuilder = new SqlCommandBuilder(myDataAdapter);
                myDataAdapter.Update(myDataSet, "timestamp");
                myDataSet.Clear();
                myConn.Close();
                flagchecked = 0;
            }
            this.Hide();
            frmMain frmmf = new frmMain();
            frmmf.Show();
        }
    }
}
