using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UserManagement.Models;
using UserManagement.Service;

namespace UserManagement.Mapping
{
    public class UserMapping
    {
        public static void MapUser(UserViewModel userVO, User user)
        {
            var Encryptpwd = new PasswordService();
            user.Active = userVO.Active;
            user.Locked = userVO.Locked;
            user.FirstName = userVO.FirstName;
            user.LastName = userVO.LastName;
            user.Email = userVO.Email;
            user.Password = Encryptpwd.EncryptPassword(userVO.Password);
        }
    }
}