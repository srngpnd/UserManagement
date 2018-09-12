using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace UserManagement.Models
{
    public class UserViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        [ScriptIgnore]
        public string Password { get; set; }
        public bool Active { get; set; }
        public bool Locked { get; set; }
        public List<string> AllowedUserAccess { get; set; }
    }
}