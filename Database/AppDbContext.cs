using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Starter_NET_7.Database.Models;
using Starter_NET_7.Database.Seeding;

namespace Starter_NET_7.Database;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        SeedingInitial.Seed(modelBuilder);
    }

    public virtual DbSet<Permission> Permissions { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<UnionPermissionsRole> UnionPermissionsRoles { get; set; }
    public virtual DbSet<UnionPermissionsUser> UnionPermissionsUsers { get; set; }
    public virtual DbSet<UserValidation> UserValidation { get; set; }
    public virtual DbSet<User> Users { get; set; }
}
