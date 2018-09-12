using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace UserManagement.Models
{
    public class UserSecurityAudit
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int AuditID { get; set; }
        public DateTime TransactionTime { get; set; }
        public string TransactionBy { get; set; }
        public string ChangeAction { get; set; }
        public int KeyID { get; set; }
        public int UserID { get; set; }
        public int AllowedUserID { get; set; }
    }
}