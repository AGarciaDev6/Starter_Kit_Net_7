using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Starter_NET_7.Database.Models.Configurations
{
  public class UnionPermissionsRoleConfig : IEntityTypeConfiguration<UnionPermissionsRole>
  {
    public void Configure(EntityTypeBuilder<UnionPermissionsRole> builder)
    {
      builder.HasKey(e => new { e.RoleId, e.PermissionId }).HasName("PK_PERMISSIONS_ROLES");

      builder.ToTable("UNION_PERMISSIONS_ROLES");

      builder.Property(e => e.RoleId).HasColumnName("ROLE_ID");
      builder.Property(e => e.PermissionId).HasColumnName("PERMISSION_ID");
      builder.Property(e => e.AssignedBy).HasColumnName("ASSIGNED_BY");
      builder.Property(e => e.AssignedDate)
          .HasColumnType("datetime")
          .HasColumnName("ASSIGNED_DATE");
      builder.Property(e => e.Status).HasColumnName("STATUS");

      builder.HasOne(d => d.Permission).WithMany(p => p.UnionPermissionsRoles)
          .HasForeignKey(d => d.PermissionId)
          .OnDelete(DeleteBehavior.ClientSetNull)
          .HasConstraintName("FK_PERMISSIONS_ROLES_PERMISSIONS");

      builder.HasOne(d => d.Role).WithMany(p => p.UnionPermissionsRoles)
          .HasForeignKey(d => d.RoleId)
          .OnDelete(DeleteBehavior.ClientSetNull)
          .HasConstraintName("FK_PERMISSIONS_ROLES_ROLES");
    }
  }
}
