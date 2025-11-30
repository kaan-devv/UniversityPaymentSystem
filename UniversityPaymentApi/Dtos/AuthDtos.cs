namespace UniversityPaymentApi.Dtos;

public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class TokenResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string Role { get; set; } = string.Empty;
}
