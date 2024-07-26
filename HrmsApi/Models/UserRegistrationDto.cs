namespace HrmsApi.Models
{
    public class UserRegistrationDto

    {

        public string Username { get; set; }
        public string Batch { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
