using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Starter_NET_7.Database.Models.Configurations
{
  public class UnionPermissionsUserConfig : IEntityTypeConfiguration<UnionPermissionsUser>
  {
    public void Configure(EntityTypeBuilder<UnionPermissionsUser> builder)
    {
      builder.HasKey(e => new { e.PermissionId, e.UserId }).HasName("PK_PERMISSIONS_USERS");

      builder.ToTable("UNION_PERMISSIONS_USERS");

      builder.Property(e => e.PermissionId).HasColumnName("PERMISSION_ID");
      builder.Property(e => e.UserId).HasColumnName("USER_ID");
      builder.Property(e => e.AssignedBy).HasColumnName("ASSIGNED_BY");
      builder.Property(e => e.AssignedDate)
          .HasColumnType("datetime")
          .HasColumnName("ASSIGNED_DATE");
      builder.Property(e => e.Status).HasColumnName("STATUS");

      builder.HasOne(d => d.Permission).WithMany(p => p.UnionPermissionsUsers)
          .HasForeignKey(d => d.PermissionId)
          .OnDelete(DeleteBehavior.ClientSetNull)
          .HasConstraintName("FK_PERMISSIONS_USERS_PERMISSIONS");

      builder.HasOne(d => d.User).WithMany(p => p.UnionPermissionsUsers)
          .HasForeignKey(d => d.UserId)
          .OnDelete(DeleteBehavior.ClientSetNull)
          .HasConstraintName("FK_PERMISSIONS_USERS_USERS");
    }
  }
}
