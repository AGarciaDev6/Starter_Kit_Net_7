using System.ComponentModel.DataAnnotations;

namespace Starter_NET_7.DTOs.Request.Profile
{
    public class ProfileRequest
    {
        public string Name { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;

        [MinLength(6)]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [MinLength(6)]
        [DataType(DataType.Password)]
        public string? PasswordConfirm { get; set; }
    }
}
