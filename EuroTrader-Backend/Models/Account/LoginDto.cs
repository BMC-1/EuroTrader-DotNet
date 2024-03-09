using System.ComponentModel.DataAnnotations;

namespace EuroTrader_Backend.Models.Account
{
    public class LoginDto
    {
        [Required]
        public string Username { get; set; }
        [Required]
        [StringLength(20,ErrorMessage = "Your password is limited to {2} to {1} characters",MinimumLength = 3)]
        public string Password { get; set; }
    }
}
