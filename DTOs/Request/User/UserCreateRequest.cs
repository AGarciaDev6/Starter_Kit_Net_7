using System.ComponentModel.DataAnnotations;

namespace Starter_NET_7.DTOs.Request.User
{
  public class UserCreateRequest
  {
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;

    [MinLength(6)]
    public string Password { get; set; } = null!;

    [MinLength(6)]
    public string PasswordConfirm { get; set; } = null!;

    public int IdRole { get; set; }
    public int[] Permissions { get; set; } = { };
  }
}
