using System;
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
using UserManagement.ViewModels;

namespace UserManagement.Controllers
{
    public class UserController : BaseController
    {
        public UserService _userService;
        public UserController()
        {
            _userService = new UserService();
        }

        [HttpPost]
        [Route("api/user")]
        public dynamic AddUser([FromBody]UserViewModel userVO)
        {
            try
            {
                var user = new User();
                UserMapping.MapUser(userVO, user);
                _dbContext.Users.Add(user);
                _dbContext.SaveChanges();
                return "successful operation";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        [Route("api/user/createWithList")]
        public dynamic CreateUsersWithListInput([FromBody]List<UserViewModel> userVOList)
        {
            try
            {
                foreach (var addedUser in userVOList)
                {
                    var user = new User();
                    UserMapping.MapUser(addedUser, user);
                    _dbContext.Users.Add(user);
                    _dbContext.SaveChanges();
                }
                return "successful operation";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        [Route("api/user/list")]
        public async Task<dynamic> ListUsers()
        {
            var user = await _dbContext.Users.ToListAsync();
            return user.Select(x => new UserViewModel
            {
                Active = x.Active,
                Locked = x.Locked,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                Password = x.Password
            });
        }

        [HttpGet]
        [Route("api/user/{email}")]
        public async Task<dynamic> GetUserByEmail([FromUri]string email)
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
                    return new UserViewModel
                    {
                        Active = user.Active,
                        Locked = user.Locked,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Password = user.Password
                    };
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

        [HttpPut]
        [Route("api/user/{email}")]
        public async Task<dynamic> UpdateUser([FromUri]string email, [FromBody]User userVO)
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

        [HttpDelete]
        [Route("api/user/{email}")]
        public async Task<dynamic> DeleteUser([FromUri]string email)
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        [Route("api/user/{email}/access")]
        public async Task<dynamic> GetUserAccessByEmail([FromUri]string email)
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

        [HttpPut]
        [Route("api/user/{email}/access")]
        public async Task<dynamic> UpdateUserAccess([FromUri]string email, [FromBody]List<string> allowedEmails)
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
                        if (dbEmails.Any(x => x.Email == item))
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
                    var exceptEmails = string.Join(",", allowedEmails.Except(dbEmails.Select(x => x.Email)));
                    if (exceptEmails.Any())
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        [Route("api/user/{email}/audit")]
        public async Task<dynamic> GetAuditByEmail([FromUri]string email)
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

    }
}