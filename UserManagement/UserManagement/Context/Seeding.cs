using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UserManagement.Models;

namespace UserManagement.Context
{
    public class Seeding
    {
        internal static void Go(DataContext context)
        {
            SeedUser(context);
        }

        private static void SeedUser(DataContext context)
        {
            var defaultEmail = "batman@batman.com";
            if (!context.Users.Any(x => x.Email == defaultEmail))
            {
                var defaultUser = new User
                {
                    Active = true,
                    Locked = false,
                    FirstName = "Bruce",
                    LastName = "Wayne",
                    Email = defaultEmail,
                    Password = "V2VsY29tZTE="
                };
                context.Users.Add(defaultUser);
                context.SaveChanges();
            }
        }
    }
}