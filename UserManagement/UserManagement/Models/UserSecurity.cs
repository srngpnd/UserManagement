using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace UserManagement.Models
{
    public class UserSecurity
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int KeyID { get; set; }
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual User User { get; set; }
        public int AllowedUserID { get; set; }

    }
}