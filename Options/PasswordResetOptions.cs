namespace StoreManagement.Options;

public class PasswordResetOptions
{
    public string AdminUrl { get; set; } = "http://localhost:3000/reset-password";

    public string ClientUrl { get; set; } = "http://localhost:3001/reset-password";

    public int TokenExpiryMinutes { get; set; } = 30;
}
