namespace Starter_NET_7.DTOs.Request.Role
{
  public class RoleRequest
  {
    public string Name { get; set; } = null!;
    public int[] Permissions { get; set; } = { };
  }
}
