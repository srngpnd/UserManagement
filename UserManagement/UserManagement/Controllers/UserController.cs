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

        [HttpPost]
        public async Task<dynamic> Post(List<UserViewModel> userVO)
        {
            try
            {
                var validatedUsers = _userService.ValidateUser(userVO);
                var allowedUsersFromVO = userVO.SelectMany(y => y.AllowedUserAccess).Distinct().ToList();
                var allowedUsers = await _dbContext.Users.Where(x => allowedUsersFromVO.Contains(x.Email)).ToListAsync();
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
                        if (allowedUsers.Any(x => x.Email == item))
                        {
                            var userSecurity = new UserSecurity()
                            {
                                UserID = user.KeyID,
                                AllowedUserID = allowedUsers.First(x => x.Email == item).KeyID
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

        [HttpPatch]
        public async Task<dynamic> Patch([FromUri]string email,[FromBody]List<string> allowedEmails)
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
                var allowedUsers = _userService.AllowedUsers(HttpContext.Current.User.Identity.Name);
                var allowedUserIDs = allowedUsers.Select(x => x.KeyID).ToList();
                var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
                if (user != null)
                {
                    if (!allowedUserIDs.Contains(user.KeyID))
                        throw new Exception("Insufficient Permissions");
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

    }
}