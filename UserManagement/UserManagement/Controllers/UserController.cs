﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using UserManagement.Mapping;
using UserManagement.Models;
using UserManagement.Service;

namespace UserManagement.Controllers
{
    public class UserController : BaseController
    {
        public UserService _userService;
        public UserController()
        {
            _userService = new UserService();
        }
        [HttpGet]
        public async Task<dynamic> List()
        {
            var user = await _dbContext.Users.ToListAsync();
            return user.Select(x => new {
                Active = x.Active,
                Locked = x.Locked,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email
            });
        }

        [HttpGet]
        public async Task<dynamic> Get([FromUri]string email)
        {
            try
            {
                var allowedUsers = _userService.AllowedUsers(HttpContext.Current.User.Identity.Name);
                var allowedUserIDs = allowedUsers.Select(x => x.KeyID).ToList();
                var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
                if (user != null)
                {
                    if (!allowedUserIDs.Contains(user.KeyID))
                        throw new Exception("Insufficient Permissions");
                    var userEmail = user.UserSecurities.Select(y => y.AllowedUserID);
                    return new
                    {
                        Active = user.Active,
                        Locked = user.Locked,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        AllowedUserAccess = _dbContext.Users.Where(x => userEmail.Contains(x.KeyID)).Select(z => z.Email).ToList()
                    };
                }
                else
                {
                    return "Enter a valid Email";
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        [HttpPut]
        public async Task<dynamic> Save(List<UserViewModel> userVO)
        {
            try
            {
                var validatedUsers = _userService.ValidateUser(userVO);
                var dbUsers = await _dbContext.Users.ToListAsync();
                foreach (var addedUser in userVO)
                {
                    var user = new User();
                    UserMapping.MapUser(addedUser, user);
                    _dbContext.Users.Add(user);
                    _dbContext.SaveChanges();
                    var userSecurities = new List<UserSecurity>();
                    addedUser.AllowedUserAccess.Add(addedUser.Email);
                    foreach (var item in addedUser.AllowedUserAccess.Distinct())
                    {
                        if (dbUsers.Any(x => x.Email == item) || addedUser.Email == item)
                        {
                            var userSecurity = new UserSecurity()
                            {
                                UserID = user.KeyID,
                                AllowedUserID = addedUser.Email == item ? user.KeyID : dbUsers.First(x => x.Email == item).KeyID
                            };
                            userSecurities.Add(userSecurity);
                        }
                    }
                    _dbContext.UserSecurities.AddRange(userSecurities);
                    _dbContext.SaveChanges();
                }
                return "Added Successfully";
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public async Task<dynamic> Post([FromUri]string email,[FromBody]UserViewModel userVO)
        {
            try
            {
                var allowedUsers = _userService.AllowedUsers(HttpContext.Current.User.Identity.Name);
                var allowedUserIDs = allowedUsers.Select(x => x.KeyID).ToList();
                var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
                if (user != null)
                {
                    if (!allowedUserIDs.Contains(user.KeyID))
                        throw new Exception("Insufficient Permissions");
                    user.Active = userVO.Active;
                    user.Locked = userVO.Locked;
                    user.FirstName = userVO.FirstName;
                    user.LastName = userVO.LastName;
                    _dbContext.SaveChanges();
                    return "Update successfull";
                }
                else
                {
                    return "Enter a valid Email";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPatch]
        public async Task<dynamic> PatchSecurities([FromUri]string email,[FromBody]List<string> allowedEmails)
        {
            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
                if (user != null)
                {
                    var currentUserSecurities = user.UserSecurities.Select(x => x.AllowedUserID).ToList();
                    var dbEmails = _dbContext.Users.Where(x => allowedEmails.Contains(x.Email) && !currentUserSecurities.Contains(x.KeyID)).ToList();
                    var userSecurities = new List<UserSecurity>();
                    foreach (var item in allowedEmails.Distinct())
                    {
                        if(dbEmails.Any(x => x.Email == item))
                        {
                            var userSecurity = new UserSecurity()
                            {
                                UserID = user.KeyID,
                                AllowedUserID = dbEmails.First(x => x.Email == item).KeyID
                            };
                            userSecurities.Add(userSecurity);
                        }
                    }
                    _dbContext.UserSecurities.AddRange(userSecurities);
                    _dbContext.SaveChanges();
                    var exceptEmails = string.Join(",",allowedEmails.Except(dbEmails.Select(x => x.Email)));
                    if(exceptEmails.Any())
                    {
                        return "Patch Successful except " + exceptEmails;
                    }
                    return "Patched Successfully";
                }
                else
                {
                    return "Enter a valid Email";
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public async Task<dynamic> UserSecurities([FromUri]string email)
        {
            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
                if (user != null)
                {
                    var userSecurityIDs = user.UserSecurities.Select(x => x.AllowedUserID).ToList();
                    var allowedUserEmails = _dbContext.Users.Where(x => userSecurityIDs.Contains(x.KeyID)).Select(x => x.Email).ToList();
                    return new { AllowedUserAccess = allowedUserEmails.ToArray() };
                }
                else
                {
                    return "Enter a valid Email";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpDelete]
        public async Task<dynamic> Delete(string email)
        {
            try
            {
                var allowedUsers = _userService.AllowedUsers(HttpContext.Current.User.Identity.Name);
                var allowedUserIDs = allowedUsers.Select(x => x.KeyID).ToList();
                var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
                if (user != null)
                {
                    if (!allowedUserIDs.Contains(user.KeyID))
                        throw new Exception("Insufficient Permissions");
                    var userSecurities = await _dbContext.UserSecurities.Where(x => x.UserID == user.KeyID).ToListAsync();
                    _dbContext.UserSecurities.RemoveRange(userSecurities);
                    _dbContext.Users.Remove(user);
                    _dbContext.SaveChanges();
                    return email + " was successfully deleted";
                }
                else
                {
                    return "Enter a valid Email";
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public async Task<dynamic> GetUserAuditLogs([FromUri]string email)
        {
            try
            {
                var allowedUsers = _userService.AllowedUsers(HttpContext.Current.User.Identity.Name);
                var allowedUserIDs = allowedUsers.Select(x => x.KeyID).ToList();
                var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
                if (user != null)
                {
                    if (!allowedUserIDs.Contains(user.KeyID))
                        throw new Exception("Insufficient Permissions");
                    var userAudits = await (from userAudit in _dbContext.UserAudits.Where(x => x.TransactionBy == email)
                                           select new
                                           {
                                               ChangeAction = userAudit.ChangeAction,
                                               TransactionTime = userAudit.TransactionTime,
                                               Active = userAudit.Active,
                                               Locked = userAudit.Locked,
                                               FirstName = userAudit.FirstName,
                                               LastName = userAudit.LastName,
                                               Email = userAudit.Email
                                           }).OrderByDescending(x => x.TransactionTime).ToListAsync();
                    return userAudits;
                }
                else
                {
                    return "Enter a valid Email";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public async Task<dynamic> GetUserSecurityAuditLogs([FromUri]string email)
        {
            try
            {
                var allowedUsers = _userService.AllowedUsers(HttpContext.Current.User.Identity.Name);
                var allowedUserIDs = allowedUsers.Select(x => x.KeyID).ToList();
                var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
                if (user != null)
                {
                    if (!allowedUserIDs.Contains(user.KeyID))
                        throw new Exception("Insufficient Permissions");
                    var userAudits = await (from userSecurityAudit in _dbContext.UserSecurityAudits.Where(x => x.TransactionBy == email)
                                            join allowedUser in _dbContext.Users on userSecurityAudit.AllowedUserID equals allowedUser.KeyID into allowed
                                            from au in allowed.DefaultIfEmpty()
                                            join mainUser in _dbContext.Users on userSecurityAudit.UserID equals user.KeyID into users
                                            from mUser in users.DefaultIfEmpty()
                                            select new
                                            {
                                                ChangeAction = userSecurityAudit.ChangeAction,
                                                TransactionID = userSecurityAudit.TransactionID,
                                                TransactionTime = userSecurityAudit.TransactionTime,
                                                ParentUser = mUser.Email,
                                                AllowedUser = au.Email
                                            }).OrderByDescending(x => x.TransactionTime).ToListAsync();
                    return userAudits;
                }
                else
                {
                    return "Enter a valid Email";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}