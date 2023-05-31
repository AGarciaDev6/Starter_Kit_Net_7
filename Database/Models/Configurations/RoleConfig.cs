using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Starter_NET_7.Database.Models.Configurations
{
    public class RoleConfig : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(e => e.IdRole);

            builder.ToTable("ROLES");

            builder.Property(e => e.IdRole).HasColumnName("ID_ROLE");
            builder.Property(e => e.CreatedBy).HasColumnName("CREATED_BY");
            builder.Property(e => e.CreationDate)
                .HasColumnType("datetime")
                .HasColumnName("CREATION_DATE");
            builder.Property(e => e.LastUpdateBy).HasColumnName("LAST_UPDATE_BY");
            builder.Property(e => e.LastUpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("LAST_UPDATE_DATE");
            builder.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("NAME");
            builder.Property(e => e.Status).HasColumnName("STATUS");
        }
    }
}
