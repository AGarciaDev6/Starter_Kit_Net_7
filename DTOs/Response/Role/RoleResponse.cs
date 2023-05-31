namespace Starter_NET_7.DTOs.Response.Role
{
    public class RoleResponse
    {
        public int IdRole { get; set; }
        public string Name { get; set; } = null!;
        public bool Status { get; set; }
        public int CreatedBy { get; set; }
        public string CreationDate { get; set; } = null!;
        public int? LastUpdateBy { get; set; }
        public string? LastUpdateDate { get; set; }
    }
}
