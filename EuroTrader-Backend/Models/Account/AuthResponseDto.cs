namespace EuroTrader_Backend.Models.Account;

public class AuthResponseDto
{
    //public int UserId { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }

    public IList<string> Roles { get; set; }
}