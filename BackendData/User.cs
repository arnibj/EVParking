using System.ComponentModel.DataAnnotations;

namespace BackendData
{
    public class User
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public User()
        {
            Name = string.Empty;
            Email = string.Empty;
            Password = string.Empty;    
        }
    }
}
