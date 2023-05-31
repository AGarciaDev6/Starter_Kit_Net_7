namespace Starter_NET_7.DTOs.Response.User
{
    public class UserWithPermissionsResponse : UserResponse
    {
        public int[] Permissions { get; set; } = null!;
    }
}
