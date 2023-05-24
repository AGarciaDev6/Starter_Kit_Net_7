namespace Starter_NET_7.Database.Models
{
  public class UserValidation
  {
    public int IdUserValidation { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiry { get; set; }

    public string? ForgotPasswordUuid { get; set; }

    public DateTime? ForgotPasswordExpiry { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
  }
}
