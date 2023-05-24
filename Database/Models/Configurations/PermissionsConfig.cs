using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace Starter_NET_7.Database.Models.Configurations
{
  public class PermissionsConfig : IEntityTypeConfiguration<Permission>
  {
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
      builder.HasKey(e => e.IdPermission);

      builder.ToTable("PERMISSIONS");

      builder.Property(e => e.IdPermission).HasColumnName("ID_PERMISSION");
      builder.Property(e => e.CreatedBy).HasColumnName("CREATED_BY");
      builder.Property(e => e.CreationDate)
          .HasColumnType("datetime")
          .HasColumnName("CREATION_DATE");
      builder.Property(e => e.LastUpdateBy).HasColumnName("LAST_UPDATE_BY");
      builder.Property(e => e.LastUpdateDate)
          .HasColumnType("datetime")
          .HasColumnName("LAST_UPDATE_DATE");
      builder.Property(e => e.Name)
          .HasMaxLength(200)
          .IsUnicode(false)
          .HasColumnName("NAME");
      builder.Property(e => e.Status).HasColumnName("STATUS");
    }
  }
}
