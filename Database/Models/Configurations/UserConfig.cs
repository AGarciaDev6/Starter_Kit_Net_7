using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Starter_NET_7.Database.Models.Configurations
{
    public class UserConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(e => e.IdUser);

            builder.ToTable("USERS");

            builder.Property(e => e.IdUser).HasColumnName("ID_USER");
            builder.Property(e => e.CreatedBy).HasColumnName("CREATED_BY");
            builder.Property(e => e.CreationDate)
                .HasColumnType("datetime")
                .HasColumnName("CREATION_DATE");
            builder.Property(e => e.Email)
                .HasMaxLength(80)
                .IsUnicode(false)
                .HasColumnName("EMAIL");
            builder.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("LAST_NAME");
            builder.Property(e => e.LastUpdateBy).HasColumnName("LAST_UPDATE_BY");
            builder.Property(e => e.LastUpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("LAST_UPDATE_DATE");
            builder.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("NAME");
            builder.Property(e => e.Password)
                .HasMaxLength(80)
                .IsUnicode(false)
                .HasColumnName("PASSWORD");
            builder.Property(e => e.RoleId).HasColumnName("ROLE_ID");
            builder.Property(e => e.Status).HasColumnName("STATUS");

            builder.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USERS_ROLES");
        }
    }
}
