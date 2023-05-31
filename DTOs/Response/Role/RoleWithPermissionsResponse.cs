namespace Starter_NET_7.DTOs.Response.Role
{
    public class RoleWithPermissionsResponse : RoleResponse
    {
        public int[] Permissions { get; set; } = null!;
    }
}
