using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WeChat.NET.caozuo
{
    //数据识别与处理
     class wxcaozuo
    {



        //获得用户昵称
        public string getname(string username)
        {
            List<string> arr = new List<string>();
            arr=Read();
            foreach (string s in arr)
            {
                int i = arr.IndexOf(s);//i就是下标
                if (arr[i].IndexOf(username)>=0)
                {
                    string nickname = arr[i].Substring(0,arr[i].IndexOf(username)).Substring (10, arr[i].Substring(10, arr[i].IndexOf(username)).Length -20);
                    return nickname;
                }
            }

            return null;
        }
        public bool panduanfenshu(string nickname, int fenshu)
        {
            return true;
        }

        //规范格式统一变成-庄-闲-庄对-闲对-和-三宝-撤-
        public string  guifan(string str)
        {
            string z = IniReadValue("下注格式","庄", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase+"set.ini");
            string x = IniReadValue("下注格式", "闲", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
            string zd = IniReadValue("下注格式", "庄对", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
            string xd = IniReadValue("下注格式", "闲对", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
            string h = IniReadValue("下注格式", "和", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
            string sb= IniReadValue("下注格式", "三宝", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
            string c = IniReadValue("下注格式", "撤", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");
            string s=IniReadValue("下注格式", "梭", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "set.ini");


            List<string> listz = filllist(z);
            List<string> listx = filllist(x);
            List<string> listzd =filllist(zd);
            List<string> listxd =filllist(xd);
            List<string> listh  =filllist(h);
            List<string> listsb =filllist(sb);
            List<string> listc  =filllist(c);
            List<string> lists = filllist(s);
            string mmmm = str;
            //判断是否有梭
            if (pd(str, lists))
            {
                foreach (string ps in lists)
                {
                    int i = lists.IndexOf(ps);//i就是下标
                    mmmm = mmmm.Replace(lists[i], "|梭：");
                }
            }
            //判断是否有庄对下注
            if (pd(str, listzd))
            {
                foreach (string ps in listzd)
                {
                    int i = listzd.IndexOf(ps);//i就是下标
                    mmmm = mmmm.Replace(listzd[i],"|庄对：");
                }
            }
            //判断是否有闲对下注
            if (pd(str, listxd))
            {
                foreach (string ps in listxd)
                {
                    int i = listxd.IndexOf(ps);//i就是下标
                    mmmm = mmmm.Replace(listxd[i], "|闲对：");
                }
            }
            //判断是否有闲下注
            if (pd(str, listx))
            {
                foreach (string ps in listx)
                {
                    int i = listx.IndexOf(ps);//i就是下标
                    mmmm = mmmm.Replace(listx[i], "|闲：");
                }
            }
            //判断是否有庄下注
            if (pd(str, listz))
            {
                foreach (string ps in listz)
                {
                    int i = listz.IndexOf(ps);//i就是下标
                    mmmm = mmmm.Replace(listz[i], "|庄：");
                }
            }
            //判断是否有和下注
            if (pd(str, listh))
            {
                foreach (string ps in listh)
                {
                    int i = listh.IndexOf(ps);//i就是下标
                    mmmm = mmmm.Replace(listh[i], "|和：");
                }
            }
            //判断是否有三宝下注
            if (pd(str, listsb))
            {
                foreach (string ps in listsb)
                {
                    int i = listsb.IndexOf(ps);//i就是下标
                    mmmm = mmmm.Replace(listsb[i], "|三宝：");
                }
            }
            //判断是否有撤
            if (pd(str, listc))
            {
                foreach (string ps in listc)
                {
                    int i = listc.IndexOf(ps);//i就是下标
                    mmmm = mmmm.Replace(listc[i], "|撤：");
                }
            }
            mmmm = mmmm.Replace("||庄：对：", "|庄对：");
            mmmm = mmmm.Replace("||闲：对：", "|闲对：");
            mmmm = mmmm.Replace("|梭：b", "|三宝：");
            Regex rex =new Regex(@"^\d+$");
            if ((rex.IsMatch(mmmm.Replace("|庄对：", "").Replace("|闲对：", "").Replace("|庄：", "").Replace("|闲：", "").Replace("|和：", "").Replace("|撤：", "").Replace("|三宝：", "").Replace ("|梭：","")))|| (mmmm.Replace("|庄对：", "").Replace("|闲对：", "").Replace("|庄：", "").Replace("|闲：", "").Replace("|和：", "").Replace("|撤：", "").Replace("|三宝：", "").Replace("|梭：", "") == ""))
            {
                return mmmm;
            }
            else
            {
                mmmm = "格式不规范";
                return mmmm;
            }

        }

        public bool pd(string str, List<string>lst)
        {
            int flag = 0;
            foreach (string ssss in lst)
            {
                int i = lst.IndexOf(ssss);//i就是下标
                if(str.IndexOf (lst[i].ToString ())>=0)
                {
                    flag = 1;
                }
            }
            if (flag == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }



        public List<string> filllist(string str )
        {
            string strsum = str;
            int mm = Regex.Matches(strsum, @"|").Count;
            List<string> arr = new List<string>();
            for (int i = 1; i <= mm; i++)
            {
                if (strsum == "")
                {
                    break;
                }
                string strr = strsum.Substring(0, strsum.IndexOf("|"));
                strsum = strsum.Remove(0, strsum.IndexOf("|") + 1);
                arr.Add(strr);
            }
            return arr;
        }


        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defVal, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        /// <summary>    
        /// 读取INI文件    
        /// </summary>    
        /// <param name="section">项目名称(如 [section] )</param>    
        /// <param name="skey">键</param>   
        /// <param name="path">路径</param> 
        public string IniReadValue(string section, string skey, string path)
        {
            StringBuilder temp = new StringBuilder(500);
            int i = GetPrivateProfileString(section, skey, "", temp, 500, path);
            return temp.ToString();
        }
        /// <summary>
        /// 写入ini文件
        /// </summary>
        /// <param name="section">项目名称</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="path">路径</param>
        public void IniWrite(string section, string key, string value, string path)
        {
            WritePrivateProfileString(section, key, value, path);
        }
        //数据读取txt
        public List<string> Read()
        {
            List<string> arr = new List<string>();
            string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase+"1.txt";
            StreamReader sr = new StreamReader(path, Encoding.Default);
            String line;
            while ((line = sr.ReadLine()) != null)
            {
                arr.Add(line .ToString ());
                
            }
            sr.Close();
            return arr;
            
        }
        //数据写入txt
        public void Write(string str , string i)
        {
            string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "1.txt";
            if (i == "Append")
            {
                FileStream fs = new FileStream(path, FileMode.Append);
                //获得字节数组
                byte[] data = System.Text.Encoding.Default.GetBytes(str);
                //开始写入
                fs.Write(data, 0, data.Length);
                //清空缓冲区、关闭流
                fs.Flush();
                fs.Close();
            }
            else
            {
                FileStream fs = new FileStream(path, FileMode.Create);
                //获得字节数组
                byte[] data = System.Text.Encoding.Default.GetBytes(str);
                //开始写入
                fs.Write(data, 0, data.Length);
                //清空缓冲区、关闭流
                fs.Flush();
                fs.Close();
            }
            
        }

        public void Delete()
        {
            string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "1.txt";
            FileStream fs = new FileStream(path, FileMode.Create);
            fs.Flush();
            fs.Close();
        }

    }
    
}
