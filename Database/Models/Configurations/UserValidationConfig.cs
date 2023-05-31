using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Starter_NET_7.Database.Models.Configurations
{
    public class UserValidationGeneralConfig : IEntityTypeConfiguration<UserValidation>
    {
        public void Configure(EntityTypeBuilder<UserValidation> builder)
        {
            builder.HasKey(e => e.IdUserValidation);

            builder.ToTable("USER_VALIDATION");

            builder.Property(e => e.IdUserValidation).HasColumnName("ID_USER_VALIDATION");
            builder.Property(e => e.RefreshToken)
                .HasMaxLength(300)
                .HasColumnName("REFRESH_TOKEN");
            builder.Property(e => e.RefreshTokenExpiry)
                .HasColumnType("datetime")
                .HasColumnName("REFRESH_TOKEN_EXPIRY");
            builder.Property(e => e.ForgotPasswordUuid)
                .HasMaxLength(300)
                .HasColumnName("FORGOT_PASSWORD_UUID");
            builder.Property(e => e.ForgotPasswordExpiry)
                .HasColumnType("datetime")
                .HasColumnName("FORGOT_PASSWORD_EXPIRY");
            builder.Property(e => e.UserId).HasColumnName("USER_ID");
        }
    }
}
