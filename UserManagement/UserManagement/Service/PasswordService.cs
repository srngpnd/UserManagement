using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace UserManagement.Service
{
    public class PasswordService
    {
        public string EncryptPassword(string password)
        {
            string msg = "";
            byte[] encode = new byte[password.Length];
            encode = Encoding.UTF8.GetBytes(password);
            msg = Convert.ToBase64String(encode);
            return msg;
        }
    }
}