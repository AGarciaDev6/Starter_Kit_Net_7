namespace Starter_NET_7.DTOs.Request.AuthController
{
  public class ResetPasswordRequest
  {
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string PasswordConfirm { get; set; } = null!;
  }
}
