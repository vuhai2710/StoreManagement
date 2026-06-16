namespace StoreManagement.Dtos.Auth;

public class AuthenticationResponseDto
{
    public string Token { get; set; } = string.Empty;

    public bool Authenticated { get; set; }
}
