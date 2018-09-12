using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using UserManagement.Models;

namespace UserManagement.Context
{
    public class DataContext : DbContext
    {
        public DataContext() : base("UserManagement")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserAudit> UserAudits { get; set; }
        public DbSet<UserSecurity> UserSecurities { get; set; }
        public DbSet<UserSecurityAudit> UserSecurityAudits { get; set; }

        public override int SaveChanges()
        {
            var entries = this.ChangeTracker.Entries().Where(p => p.State == EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified);
            var userAuditList = new List<UserAudit>();
            var userSecurityAuditList = new List<UserSecurityAudit>();
            foreach (var entry in entries)
            {
                if(entry.Entity.GetType() == typeof(User))
                {
                    var user = entry.Cast<User>();
                    userAuditList.Add(new UserAudit {
                        ChangeAction = entry.State.ToString(),
                        TransactionBy = HttpContext.Current.User != null ? HttpContext.Current.User.Identity.Name : "System",
                        TransactionTime = DateTime.Now,
                        KeyID = user.Entity.KeyID,
                        Active = user.Entity.Active,
                        Locked = user.Entity.Locked,
                        FirstName = user.Entity.FirstName,
                        LastName = user.Entity.LastName,
                        Email = user.Entity.Email,
                        Password = user.Entity.Password
                    });
                }
                else
                    if(entry.Entity.GetType() == typeof(UserSecurity))
                {
                    var userSecurity = entry.Cast<UserSecurity>();
                    userSecurityAuditList.Add(new UserSecurityAudit
                    {
                        ChangeAction = entry.State.ToString(),
                        TransactionBy = HttpContext.Current.User != null ? HttpContext.Current.User.Identity.Name : "System",
                        TransactionTime = DateTime.Now,
                        KeyID = userSecurity.Entity.KeyID,
                        UserID = userSecurity.Entity.UserID,
                        AllowedUserID = userSecurity.Entity.AllowedUserID
                    });
                }
            }
            this.UserAudits.AddRange(userAuditList);
            this.UserSecurityAudits.AddRange(userSecurityAuditList);
            return base.SaveChanges();
        }
    }
}