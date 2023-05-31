using Starter_NET_7.DTOs.Response.User;

namespace Starter_NET_7.DTOs.Response.Auth
{
    public class LoginResponse
    {
        public AuthorizationResponse Authorization { get; set; } = null!;
        public UserCompactResponse User { get; set; } = null!;
    }
}
