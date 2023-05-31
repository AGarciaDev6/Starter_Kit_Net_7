using Starter_NET_7.DTOs.Response.Role;

namespace Starter_NET_7.DTOs.Response.User
{
    public class UserResponse
    {
        public int IdUser { get; set; }
        public string Name { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool Status { get; set; }
        public int CreatedBy { get; set; }
        public string CreationDate { get; set; } = null!;
        public int? LastUpdateBy { get; set; }
        public string? LastUpdateDate { get; set; }
        public RoleCompactResponse Role { get; set; } = null!;
    }
}
