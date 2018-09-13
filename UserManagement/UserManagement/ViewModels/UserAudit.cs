namespace UserManagement.ViewModels
{ 
    public class UserAudit
    { 
        public string ChangeAction { get; set; }
        public string TransactionTime { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool? Active { get; set; }
        public bool? Locked { get; set; }

    }
}
