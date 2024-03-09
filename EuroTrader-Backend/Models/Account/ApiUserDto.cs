using System.ComponentModel.DataAnnotations;

namespace EuroTrader_Backend.Models.Account;

public class ApiUserDto : LoginDto
{
    [Required]
    public string Username { get; set; }
    
    [EmailAddress,Required]
    public string Email { get; set; }
    
    [Required]
    public string Role { get; set; }
}