using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WeChat.NET
{
    public partial class shangxiafen : Form
    {
        public shangxiafen()
        {
            InitializeComponent();
        }
        //数据库连接语句
        public string connstr = "Data Source =.; Integrated Security = SSPI; Initial Catalog = database";
        private void shangxiafen_Load(object sender, EventArgs e)
        {
            label2.Text = frmMain.ssusername;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SqlConnection myConn = new SqlConnection(connstr);//创建数据库连接类的对象
            myConn.Open(); //将连接打开
            SqlDataAdapter myDataAdapter = new SqlDataAdapter("select * from shishishuju where nickname= '" + frmMain.ssusername + "'", myConn);
            DataSet myDataSet = new DataSet();
            myDataAdapter.Fill(myDataSet, "shishishuju");
            DataTable myTable = myDataSet.Tables["shishishuju"];
            foreach (DataRow myRow in myTable.Rows)
            {
                myRow["shenyufenshu"] = Convert.ToInt32(myRow["shenyufenshu"]) + Convert.ToInt32(textBox1.Text);
            }
            SqlCommandBuilder mySqlCommandBuilder = new SqlCommandBuilder(myDataAdapter);
            myDataAdapter.Update(myDataSet, "shishishuju");
            myDataSet.Clear();
            myConn.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SqlConnection myConn = new SqlConnection(connstr);//创建数据库连接类的对象
            myConn.Open(); //将连接打开
            SqlDataAdapter myDataAdapter = new SqlDataAdapter("select * from shishishuju where nickname= '" + frmMain.ssusername + "'", myConn);
            DataSet myDataSet = new DataSet();
            myDataAdapter.Fill(myDataSet, "shishishuju");
            DataTable myTable = myDataSet.Tables["shishishuju"];
            foreach (DataRow myRow in myTable.Rows)
            {
                if ((Convert.ToInt32(myRow["shenyufenshu"]) - Convert.ToInt32(textBox1.Text)) >= 0)
                {
                    myRow["shenyufenshu"] = Convert.ToInt32(myRow["shenyufenshu"]) - Convert.ToInt32(textBox1.Text);
                }
                
            }
            SqlCommandBuilder mySqlCommandBuilder = new SqlCommandBuilder(myDataAdapter);
            myDataAdapter.Update(myDataSet, "shishishuju");
            myDataSet.Clear();
            myConn.Close();

            frmMain  frmmf = new frmMain();
        }
    }
}
