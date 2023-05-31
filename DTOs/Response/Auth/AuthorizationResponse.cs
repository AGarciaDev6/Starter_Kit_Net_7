namespace Starter_NET_7.DTOs.Response.Auth
{
    public class AuthorizationResponse
    {
        public string TokenType { get; set; } = "Bearer";
        public string Token { get; set; } = null!;
        public string ExpiresAt { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public string RefreshExpiresAt { get; set; } = null!;
    }
}
