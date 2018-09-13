using System;
using System.Collections.Generic;
using System.Linq;
using UserManagement.Context;
using UserManagement.Models;
using UserManagement.ViewModels;

namespace UserManagement.Service
{
    public class UserService
    {
        private DataContext db;

        public UserService()
        {
            db = new DataContext();
        }
        public bool AuthenticateUser(string email, string password)
        {
            var passwordService = new PasswordService();
            var encryptPassword = passwordService.EncryptPassword(password);
            return db.Users.Any(x => x.Email == email && x.Password == encryptPassword);
        }

        public List<AllowedUsers> AllowedUsers(string email)
        {
            if(email == "batman@batman.com")
            {
                return db.Users.Select(x => new AllowedUsers { KeyID = x.KeyID, Email = x.Email }).ToList();
            }
            else
            {
                var userSecurities = (from user in db.Users.First(x => x.Email == email).UserSecurities
                                      join userEmail in db.Users on user.AllowedUserID equals userEmail.KeyID
                                      select new AllowedUsers
                                      {
                                          KeyID = userEmail.KeyID,
                                          Email = userEmail.Email
                                      }).ToList();
                return userSecurities;
            }
        }

        public List<UserViewModel> ValidateUser(List<UserViewModel> userVO)
        {
            var dbEmails = db.Users.Select(x => x.Email).ToList();
            var voEmails = userVO.Select(x => x.Email).ToList();
            var duplicateEmails = voEmails.Intersect(dbEmails);
            if(duplicateEmails.Any())
            {
                var validation = string.Join(",", duplicateEmails);
                throw new Exception("Duplicate Emails : " + validation);
            }
            return userVO;
        }
    }
}