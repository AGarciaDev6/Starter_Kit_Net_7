using Starter_NET_7.DTOs.Response.Permission;
using Starter_NET_7.DTOs.Response.Role;

namespace Starter_NET_7.DTOs.Response.User
{
  public class UserCompactResponse
  {
    public int IdUser { get; set; }
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public IEnumerable<PermissionCompactResponse> Permissions { get; set; } = new List<PermissionCompactResponse>();
    public RoleCompactResponse Role { get; set; } = null!;
  }
}
