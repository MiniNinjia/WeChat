using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace User
{
    public class UserModel
    {
        public UserModel()
        { }
        /// <summary>
        ///用户的唯一标识
        /// </summary>
        private string username;

        public string Username
        {
            get
            {
                return username;
            }

            set
            {
                username = value;
            }
        }
        /// <summary>
        /// 用户的昵称
        /// </summary>
        private string nickname;

        public string Nickname
        {
            get
            {
                return nickname;
            }

            set
            {
                nickname = value;
            }
        }
    }
}