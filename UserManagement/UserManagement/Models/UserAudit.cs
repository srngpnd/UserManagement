using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace UserManagement.Models
{
    public class UserAudit
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int AuditID { get; set; }
        public DateTime TransactionTime { get; set; }
        public string TransactionBy { get; set; }
        public string ChangeAction { get; set; }
        public int KeyID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Active { get; set; }
        public bool Locked { get; set; }
    }
}