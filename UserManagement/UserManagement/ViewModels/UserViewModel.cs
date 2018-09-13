namespace UserManagement.ViewModels
{ 
    public class UserViewModel
    { 
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Active { get; set; }
        public bool Locked { get; set; }
    }
}
