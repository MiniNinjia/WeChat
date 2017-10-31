using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.IO;
using System.Net;
using System.Collections.Specialized;
using System.Collections;

namespace WeChat.NET.HTTP
{
    /// <summary>
    /// 微信主要业务逻辑服务类
    /// </summary>
    class WXService
    {
        private static Dictionary<string, string> _syncKey = new Dictionary<string, string>();

        //微信初始化url
        private static string _init_url = "https://" + LoginService.Url + ".qq.com/cgi-bin/mmwebwx-bin/webwxinit?r=1377482058764";
        //获取好友头像
        private static string _geticon_url = "https://" + LoginService.Url + ".qq.com/cgi-bin/mmwebwx-bin/webwxgeticon?username=";
        //获取群聊（组）头像                             
        private static string _getheadimg_url = "https://" + LoginService.Url + ".qq.com/cgi-bin/mmwebwx-bin/webwxgetheadimg?username=";
        //获取好友列表                                   
        private static string _getcontact_url = "https://" + LoginService.Url + ".qq.com/cgi-bin/mmwebwx-bin/webwxgetcontact";
        //同步检查url                                 
        private static string _synccheck_url = "https://webpush.weixin.qq.com/cgi-bin/mmwebwx-bin/synccheck?sid={0}&uin={1}&synckey={2}&r={3}&skey={4}&deviceid={5}";
        //同步检查url                                 
        private static string _synccheck_url2 = "https://webpush.wx2.qq.com/cgi-bin/mmwebwx-bin/synccheck?sid={0}&uin={1}&synckey={2}&r={3}&skey={4}&deviceid={5}";
        //同步url                                 
        private static string _sync_url =       "https://"+LoginService .Url+".qq.com/cgi-bin/mmwebwx-bin/webwxsync?sid=";
        //发送消息url                                    
        private static string _sendmsg_url =    "https://"+LoginService .Url+".qq.com/cgi-bin/mmwebwx-bin/webwxsendmsg?sid=";
        //获取群聊成员                                  
        private static string _group_url =      "https://"+LoginService .Url+".qq.com/cgi-bin/mmwebwx-bin/webwxbatchgetcontact?type=ex";
        //图片上传服务器https://file.wx.qq.com/cgi-bin/mmwebwx-bin/webwxuploadmedia?f=json
        private static string _image_url = "https://file." + LoginService.Url + ".qq.com/cgi-bin/mmwebwx-bin/webwxuploadmedia?f=json";
        /// <summary>
        /// 微信初始化
        /// </summary>
        /// <returns></returns>
        public JObject WxInit()
        {
            string init_json = "{{\"BaseRequest\":{{\"Uin\":\"{0}\",\"Sid\":\"{1}\",\"Skey\":\"{2}\",\"DeviceID\":\"e831645047756137\"}}}}";
            Cookie sid = BaseService.GetCookie("wxsid");
            Cookie uin = BaseService.GetCookie("wxuin");

            if (sid != null && uin != null)
            {
                init_json = string.Format(init_json, uin.Value, sid.Value, LoginService.SKey);
                byte[] bytes = BaseService.SendPostRequest(_init_url + "&pass_ticket=" + LoginService.Pass_Ticket, init_json);
                string init_str = Encoding.UTF8.GetString(bytes);

                JObject init_result = JsonConvert.DeserializeObject(init_str) as JObject;

                foreach (JObject synckey in init_result["SyncKey"]["List"])  //同步键值
                {
                    _syncKey.Add(synckey["Key"].ToString(), synckey["Val"].ToString());
                }
                return init_result;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 获取好友头像
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public Image GetIcon(string username)
        {
            byte[] bytes = BaseService.SendGetRequest(_geticon_url + username);

            return Image.FromStream(new MemoryStream(bytes));
        }
        /// <summary>
        /// 获取微信讨论组头像
        /// </summary>
        /// <param name="usename"></param>
        /// <returns></returns>
        public Image GetHeadImg(string usename)
        {
            byte[] bytes = BaseService.SendGetRequest(_getheadimg_url + usename);
            return Image.FromStream(new MemoryStream(bytes));
        }
        /// <summary>
        /// 获取好友列表
        /// </summary>
        /// <returns></returns>
       public JObject GetContact()
       {
           byte[] bytes = BaseService.SendGetRequest(_getcontact_url);
           string contact_str = Encoding.UTF8.GetString(bytes);
       
           return JsonConvert.DeserializeObject(contact_str) as JObject;     
       }
        /// <summary>
        /// 获取群聊成员列表
        /// </summary>
        /// <returns></returns>
        public JObject GetGroupItem(string groupid)
     {
        Cookie sid = BaseService.GetCookie("wxsid");
        Cookie uin = BaseService.GetCookie("wxuin");
        if (sid != null && uin != null)
        {
            string data = "{\"List\":[{\"UserName\":\"" + groupid + "\",\"EncryChatRoomId\":\"\"}],\"Count\":1,\"BaseRequest\":{\"Uin\":" + uin.Value + ",\"Sid\":\"" + sid.Value + "\",\"Skey\":\"" + LoginService.SKey + "\",\"DeviceID\":\"" + "e831645047756137" + "\"}}";
            string urll = _group_url + "&r=" + (long)(DateTime.Now.ToUniversalTime() - new System.DateTime(1970, 1, 1)).TotalMilliseconds + "&pass_ticket=" + LoginService.Pass_Ticket;
            byte[] bytes1 = BaseService.SendPostRequest(urll, data);
            string contact_str = Encoding.UTF8.GetString(bytes1);
            return JsonConvert.DeserializeObject(contact_str) as JObject;
        }
         else
         {
             return null;
         }
        }
        /// <summary>
        /// 微信同步检测
        /// </summary>
        /// <returns></returns>
        public string WxSyncCheck()
        {
            string sync_key = "";
            foreach (KeyValuePair<string, string> p in _syncKey)
            {
                sync_key += p.Key + "_" + p.Value + "%7C";
            }
            sync_key = sync_key.TrimEnd('%','7','C');

            Cookie sid = BaseService.GetCookie("wxsid");
            Cookie uin = BaseService.GetCookie("wxuin");


            if (LoginService .Url.ToString().IndexOf("2") >= 0)
            {
                _synccheck_url = _synccheck_url2;
             }
            if (sid != null && uin != null)
            {
                _synccheck_url = string.Format(_synccheck_url, sid.Value, uin.Value, sync_key, (long)(DateTime.Now.ToUniversalTime() - new System.DateTime(1970, 1, 1)).TotalMilliseconds, LoginService.SKey.Replace("@", "%40"), "e1615250492");

                byte[] bytes = BaseService.SendGetRequest(_synccheck_url +"&_=" + DateTime.Now.Ticks);
                if (bytes != null)
                {
                    return Encoding.UTF8.GetString(bytes);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 微信同步
        /// </summary>
        /// <returns></returns>
        public JObject WxSync()
        {
            string sync_json = "{{\"BaseRequest\" : {{\"DeviceID\":\"e831645047756137\",\"Sid\":\"{1}\", \"Skey\":\"{5}\", \"Uin\":\"{0}\"}},\"SyncKey\" : {{\"Count\":{2},\"List\":[{3}]}},\"rr\" :{4}}}";
            Cookie sid = BaseService.GetCookie("wxsid");
            Cookie uin = BaseService.GetCookie("wxuin");

            string sync_keys = "";
            foreach (KeyValuePair<string, string> p in _syncKey)
            {
                sync_keys += "{\"Key\":" + p.Key + ",\"Val\":" + p.Value + "},";
            }
            sync_keys = sync_keys.TrimEnd(',');
            sync_json = string.Format(sync_json, uin.Value, sid.Value, _syncKey.Count, sync_keys, (long)(DateTime.Now.ToUniversalTime() - new System.DateTime(1970, 1, 1)).TotalMilliseconds, LoginService.SKey);

            if (sid != null && uin != null)
            {
                byte[] bytes = BaseService.SendPostRequest(_sync_url + sid.Value + "&lang=zh_CN&skey=" + LoginService.SKey + "&pass_ticket=" + LoginService.Pass_Ticket, sync_json);
                string sync_str = Encoding.UTF8.GetString(bytes);

                JObject sync_resul = JsonConvert.DeserializeObject(sync_str) as JObject;

                if (sync_resul["SyncKey"]["Count"].ToString() != "0")
                {
                    _syncKey.Clear();
                    foreach (JObject key in sync_resul["SyncKey"]["List"])
                    {
                        _syncKey.Add(key["Key"].ToString(), key["Val"].ToString());
                    }
                }
                return sync_resul;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="type"></param>
        public void SendMsg(string msg, string from, string to, int type)
        {
            string msg_json = "{{" +
            "\"BaseRequest\":{{" +
                "\"DeviceID\" : \"e831645047756137\"," +
                "\"Sid\" : \"{0}\"," +
                "\"Skey\" : \"{6}\"," +
                "\"Uin\" : \"{1}\"" +
            "}}," +
            "\"Msg\" : {{" +
                "\"ClientMsgId\" : {8}," +
                "\"Content\" : \"{2}\"," +
                "\"FromUserName\" : \"{3}\"," +
                "\"LocalID\" : {9}," +
                "\"ToUserName\" : \"{4}\"," +
                "\"Type\" : {5}" +
            "}}," +
            "\"rr\" : {7}" +
            "}}";

            Cookie sid = BaseService.GetCookie("wxsid");
            Cookie uin = BaseService.GetCookie("wxuin");

            if (sid != null && uin != null)
            {
                msg_json = string.Format(msg_json, sid.Value, uin.Value, msg, from, to, type, LoginService.SKey, DateTime.Now.Millisecond, DateTime.Now.Millisecond, DateTime.Now.Millisecond);

                byte[] bytes = BaseService.SendPostRequest(_sendmsg_url + sid.Value + "&lang=zh_CN&pass_ticket="+LoginService.Pass_Ticket, msg_json);

                string send_result = Encoding.UTF8.GetString(bytes);
            }
        }
        private static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail, error:" +ex.Message);
            }
        }
        /// <summary>
        /// 发送图片,gif格式
        /// </summary>
        /// <param name="ToUserName"></param>
        /// <param name="filePath"></param>
        public void Wx_SendFile_Gif(string ToUserName, string filePath)
        {
            Cookie sid = BaseService.GetCookie("wxsid");
            Cookie uin = BaseService.GetCookie("wxuin");
            Cookie webwx_data_ticket = BaseService.GetCookie("webwx_data_ticket");
            string[] fileTypes = new string[] { "image/jpeg", "image/png", "image/bmp", "image/jpeg", "text/plain", "application/msword", "application/vnd.ms-excel" };
            string[] mediaTypes = new string[] { "pic", "pic", "pic", "doc", "doc", "doc" };

            //string ClientMsgId = DateTime.Now.Millisecond.ToString();
            long ClientMsgId = (long)(DateTime.Now.ToUniversalTime() - new System.DateTime(1970, 1, 1)).TotalMilliseconds;
            FileInfo file = new FileInfo(filePath);
            if (!file.Exists)
            {
                return;
            }
            string fileMd5 = GetMD5HashFromFile(file.FullName);
            byte[] fileData = File.ReadAllBytes(file.FullName);


            #region data
            string data = @"------WebKitFormBoundaryqmAlcppnh4tFP6al
Content-Disposition: form-data; name=""id""

WU_FILE_0
------WebKitFormBoundaryqmAlcppnh4tFP6al
Content-Disposition: form-data; name=""name""

" + file.FullName + @"
------WebKitFormBoundaryqmAlcppnh4tFP6al
Content-Disposition: form-data; name=""type""

" + "image/gif" + @"
------WebKitFormBoundaryqmAlcppnh4tFP6al
Content-Disposition: form-data; name=""lastModifiedDate""

" + DateTime.Now.ToString("r").Replace(",", "") + @"+0800
------WebKitFormBoundaryqmAlcppnh4tFP6al
Content-Disposition: form-data; name=""size""

" + fileData.Length.ToString() + @"
------WebKitFormBoundaryqmAlcppnh4tFP6al
Content-Disposition: form-data; name=""mediatype""

" + "doc" + @"
------WebKitFormBoundaryqmAlcppnh4tFP6al
Content-Disposition: form-data; name=""uploadmediarequest""

{""UploadType"":2,""BaseRequest"":{""Uin"":" + uin.Value + @",""Sid"":""" + sid.Value + @""",""Skey"":""" + LoginService.SKey + @""",""DeviceID"":""" + "e831645047756137" + @"""},""ClientMediaId"":" + ClientMsgId + @",""TotalLen"":" + fileData.Length.ToString() + @",""StartPos"":0,""DataLen"":" + fileData.Length.ToString() + @",""MediaType"":4,""FromUserName"":""" + frmMain.usernameid + @""",""ToUserName"":""" + ToUserName + @""",""FileMd5"":""" + fileMd5 + @"""}
------WebKitFormBoundaryqmAlcppnh4tFP6al
Content-Disposition: form-data; name=""webwx_data_ticket""

" + webwx_data_ticket.Value + @"
------WebKitFormBoundaryqmAlcppnh4tFP6al
Content-Disposition: form-data; name=""pass_ticket""

" + LoginService.Pass_Ticket + @"
------WebKitFormBoundaryqmAlcppnh4tFP6al
Content-Disposition: form-data; name=""filename""; filename=""" + file.Name + @"""
Content-Type: " + "image/gif" + @"

";
            #endregion
            byte[] postData = Encoding.UTF8.GetBytes(data);
            byte[] endData = Encoding.UTF8.GetBytes("\r\n------WebKitFormBoundaryqmAlcppnh4tFP6al--\r\n");

            List<byte[]> lt = new List<byte[]>();
            lt.Add(postData);
            lt.Add(fileData);
            lt.Add(endData);

            byte[] tmp = new byte[postData.Length + fileData.Length];
            System.Buffer.BlockCopy(postData, 0, tmp, 0, postData.Length);
            System.Buffer.BlockCopy(fileData, 0, tmp, postData.Length, fileData.Length);
            byte[] body = new byte[tmp.Length + endData.Length];
            System.Buffer.BlockCopy(tmp, 0, body, 0, tmp.Length);
            System.Buffer.BlockCopy(endData, 0, body, tmp.Length, endData.Length);
            string url = _image_url;
            byte[] bytes = BaseService.SendPostRequestByByte(url, body, "----WebKitFormBoundaryqmAlcppnh4tFP6al");
            string send_result = Encoding.UTF8.GetString(bytes);
            if (send_result.IndexOf("\"Ret\": 1,") > 0)
            {
                return;
            }

            /////string ret = http.PostBytes(url, lt, "https://" + info.fun + ".qq.com/", "*/*", "multipart/form-data; boundary=----WebKitFormBoundaryqmAlcppnh4tFP6al");
            /////
            /////Hashtable json = (Hashtable)ClsJson.Decode(ret);
            /////if (json == null)
            /////{
            /////    return;
            /////}
            string MediaId = send_result.Substring(send_result.IndexOf("\"MediaId\":"), send_result.IndexOf("\"StartPos\":") - send_result.IndexOf("\"MediaId\":")).Replace("\"MediaId\": \"", "").Replace("\",", "").Replace(" ", "").Replace("\n", "");
            //SendMsg(MediaId, frmMain.usernameid, ToUserName,3);
            Wx_SendGifByMediaId(ToUserName, MediaId);
        }
        /// <summary>
        /// 
        /// 发送图片,png格式
        /// </summary>
        /// <param name="ToUserName"></param>
        /// <param name="filePath"></param>
        public void Wx_SendFile(string ToUserName, string filePath)
        {
            Cookie sid = BaseService.GetCookie("wxsid");
            Cookie uin = BaseService.GetCookie("wxuin");
            Cookie webwx_data_ticket = BaseService.GetCookie("webwx_data_ticket");
            string[] fileTypes = new string[] { "image/jpeg", "image/png", "image/bmp", "image/jpeg", "text/plain", "application/msword", "application/vnd.ms-excel" };
            string[] mediaTypes = new string[] { "pic", "pic", "pic", "doc", "doc", "doc" };

            //string ClientMsgId = DateTime.Now.Millisecond.ToString();
            long ClientMsgId =(long) (DateTime.Now.ToUniversalTime() - new System.DateTime(1970, 1, 1)).TotalMilliseconds;
            FileInfo file = new FileInfo(filePath);
            if (!file.Exists)
            {
                return;
            }
            string fileMd5 = GetMD5HashFromFile(file.FullName);
            byte[] fileData = File.ReadAllBytes(file.FullName);


            #region data
            string data = @"------WebKitFormBoundaryqmAlcppnh4tFP6al
Content-Disposition: form-data; name=""id""

WU_FILE_0
------WebKitFormBoundaryqmAlcppnh4tFP6al
Content-Disposition: form-data; name=""name""

" + file.FullName + @"
------WebKitFormBoundaryqmAlcppnh4tFP6al
Content-Disposition: form-data; name=""type""

" + "image/png" + @"
------WebKitFormBoundaryqmAlcppnh4tFP6al
Content-Disposition: form-data; name=""lastModifiedDate""

" + DateTime.Now.ToString("r").Replace (",","") + @"+0800
------WebKitFormBoundaryqmAlcppnh4tFP6al
Content-Disposition: form-data; name=""size""

" + fileData.Length.ToString() + @"
------WebKitFormBoundaryqmAlcppnh4tFP6al
Content-Disposition: form-data; name=""mediatype""

" + "pic" + @"
------WebKitFormBoundaryqmAlcppnh4tFP6al
Content-Disposition: form-data; name=""uploadmediarequest""

{""UploadType"":2,""BaseRequest"":{""Uin"":" + uin.Value + @",""Sid"":""" + sid.Value + @""",""Skey"":""" + LoginService.SKey + @""",""DeviceID"":""" + "e831645047756137" + @"""},""ClientMediaId"":" + ClientMsgId + @",""TotalLen"":" + fileData.Length.ToString() + @",""StartPos"":0,""DataLen"":" + fileData.Length.ToString() + @",""MediaType"":4,""FromUserName"":""" + frmMain.usernameid + @""",""ToUserName"":""" + ToUserName + @""",""FileMd5"":""" + fileMd5 + @"""}
------WebKitFormBoundaryqmAlcppnh4tFP6al
Content-Disposition: form-data; name=""webwx_data_ticket""

" + webwx_data_ticket.Value  + @"
------WebKitFormBoundaryqmAlcppnh4tFP6al
Content-Disposition: form-data; name=""pass_ticket""

" + LoginService.Pass_Ticket + @"
------WebKitFormBoundaryqmAlcppnh4tFP6al
Content-Disposition: form-data; name=""filename""; filename=""" + file.Name + @"""
Content-Type: " + "image/png" + @"

";
            #endregion
            byte[] postData = Encoding.UTF8.GetBytes(data);
            byte[] endData = Encoding.UTF8.GetBytes("\r\n------WebKitFormBoundaryqmAlcppnh4tFP6al--\r\n");

            List<byte[]> lt = new List<byte[]>();
            lt.Add(postData);
            lt.Add(fileData);
            lt.Add(endData);

            byte[] tmp = new byte[postData.Length + fileData.Length];
            System.Buffer.BlockCopy(postData, 0, tmp, 0, postData.Length);
            System.Buffer.BlockCopy(fileData, 0, tmp, postData.Length, fileData.Length);
            byte[] body = new byte[tmp.Length + endData.Length];
            System.Buffer.BlockCopy(tmp, 0, body, 0, tmp.Length);
            System.Buffer.BlockCopy(endData, 0, body, tmp.Length, endData.Length);
            string url = _image_url;
            byte[] bytes = BaseService.SendPostRequestByByte(url, body, "----WebKitFormBoundaryqmAlcppnh4tFP6al");
            string send_result = Encoding.UTF8.GetString(bytes);
            if (send_result.IndexOf ("\"Ret\": 1,")>0)
            {
                return;
            }

            /////string ret = http.PostBytes(url, lt, "https://" + info.fun + ".qq.com/", "*/*", "multipart/form-data; boundary=----WebKitFormBoundaryqmAlcppnh4tFP6al");
            /////
            /////Hashtable json = (Hashtable)ClsJson.Decode(ret);
            /////if (json == null)
            /////{
            /////    return;
            /////}
            string MediaId = send_result.Substring (send_result.IndexOf("\"MediaId\":"), send_result.IndexOf("\"StartPos\":") - send_result.IndexOf("\"MediaId\":")).Replace ("\"MediaId\": \"","").Replace ("\",","").Replace (" ","").Replace ("\n", "");
            //SendMsg(MediaId, frmMain.usernameid, ToUserName,3);
            Wx_SendPicByMediaId(ToUserName, MediaId);
        }
        /// <summary>
        /// 发送pic图片
        /// </summary>
        /// <param name="ToUserName"></param>
        /// <param name="MediaId"></param>
       public void Wx_SendPicByMediaId(string ToUserName, string MediaId)
       {
            Cookie sid = BaseService.GetCookie("wxsid");
            Cookie uin = BaseService.GetCookie("wxuin");
            string MsgType = "3";
           string Content = "";
           string ClientMsgId = DateTime.Now.Millisecond.ToString();
           string url  = "https://" + LoginService.Url + ".qq.com/cgi-bin/mmwebwx-bin/webwxsendmsgimg?fun=async&f=json&lang=zh_CN&pass_ticket="+LoginService.Pass_Ticket;
           string data = "{\"BaseRequest\":{\"Uin\":" + uin.Value  + ",\"Sid\":\"" +sid.Value + "\",\"Skey\":\"" + LoginService.SKey + "\",\"DeviceID\":\"" + "e831645047756137" + "\"},\"Msg\":{\"Type\":" + MsgType + ",\"MediaId\":\"" + MediaId + "\",\"Content\":\"" + Content + "\",\"FromUserName\":\"" + frmMain.usernameid + "\",\"ToUserName\":\"" + ToUserName + "\",\"LocalID\":\"" + ClientMsgId + "\",\"ClientMsgId\":\"" + ClientMsgId + "\"},\"Scene\":0}";
            byte [] bytes = BaseService.SendPostRequest(url,data);
            string send_result = Encoding.UTF8.GetString(bytes);
        }
        /// <summary>
        /// 发送动态图
        /// </summary>
        /// <param name="ToUserName"></param>
        /// <param name="MediaId"></param>
        public void Wx_SendGifByMediaId(string ToUserName, string MediaId)
        {
            Random ran = new Random();
            int RandKey = ran.Next(1000, 9999);
            Cookie sid = BaseService.GetCookie("wxsid");
            Cookie uin = BaseService.GetCookie("wxuin");
            string MsgType = "47";
            string EmojiFlag = "2";
            string ClientMsgId = ((long)((DateTime.Now.ToUniversalTime() - new System.DateTime(1970, 1, 1)).TotalMilliseconds)).ToString () + RandKey.ToString ();
            string url = "https://" + LoginService.Url + ".qq.com/cgi-bin/mmwebwx-bin/webwxsendemoticon?fun=sys&lang=zh_CN&pass_ticket=" + LoginService.Pass_Ticket;
            string data = "{\"BaseRequest\":{\"Uin\":" + uin.Value + ",\"Sid\":\"" + sid.Value + "\",\"Skey\":\"" + LoginService.SKey + "\",\"DeviceID\":\"" + "e831645047756137" + "\"},\"Msg\":{\"Type\":" + MsgType + ",\"EmojiFlag\":" + EmojiFlag + ",\"MediaId\":\"" + MediaId +  "\",\"FromUserName\":\"" + frmMain.usernameid + "\",\"ToUserName\":\"" + ToUserName + "\",\"LocalID\":\"" + ClientMsgId + "\",\"ClientMsgId\":\"" + ClientMsgId + "\"},\"Scene\":0}";
            byte[] bytes = BaseService.SendPostRequest(url, data);
            string send_result = Encoding.UTF8.GetString(bytes);
        }



    }
}
