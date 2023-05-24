using Starter_NET_7.Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Starter_NET_7.Database.Seeding
{
  public class SeedingInitial
  {
    public static void Seed(ModelBuilder modelBuilder)
    {
      /*
       * Seeding Roles
       */
      int id = 1;
      string[] rolesName = new string[]
      {
                "Root",
                "Admin"
      };

      foreach (string roleName in rolesName)
      {
        Role role = new Role()
        {
          IdRole = id++,
          Name = roleName,
          Status = true,
          CreatedBy = 1,
          CreationDate = DateTime.Now,
        };

        modelBuilder.Entity<Role>().HasData(role);
      }

      /*
       * Seeding User
       */
      User user = new User()
      {
        IdUser = 1,
        Name = "Super",
        LastName = "User",
        Email = "superuser@mail.com",
        Password = "$2a$11$aMKwWQmIv5iVvOY/ysJhK.OrZ4mifA0fGw7uELtr2MkOgwkE5Fm22",
        Status = true,
        CreatedBy = 1,
        CreationDate = DateTime.Now,
        RoleId = 1,
      };

      modelBuilder.Entity<User>().HasData(user);

      UserValidation userValidation = new UserValidation() { IdUserValidation = 1, UserId = user.IdUser };
      modelBuilder.Entity<UserValidation>().HasData(userValidation);

      /*
       * Seeding Permissions
       */
      string[] permissionsName = new string[]
      {
                "Roles",
                "Users",
      };

      id = 1;
      foreach (string permissionName in permissionsName)
      {
        Permission permission = new Permission()
        {
          IdPermission = id++,
          Name = permissionName,
          Status = true,
          CreatedBy = 1,
          CreationDate = DateTime.Now,
        };

        modelBuilder.Entity<Permission>().HasData(permission);
      }

      /*
       * Seeding Permission of role
       */
      List<UnionPermissionsRole> permissionsRoles = new List<UnionPermissionsRole>()
            {
                // Role: Root
                new UnionPermissionsRole() { PermissionId = 1, RoleId = 1 }, // Permission: Roles
                new UnionPermissionsRole() { PermissionId = 2, RoleId = 1 }, // Permission: Users

                // Role: Admin
                new UnionPermissionsRole() { PermissionId = 1, RoleId = 2 }, // Permission: Roles
                new UnionPermissionsRole() { PermissionId = 2, RoleId = 2 }, // Permission: Users
            };

      foreach (var permissions in permissionsRoles)
      {
        permissions.Status = true;
        permissions.AssignedBy = 1;
        permissions.AssignedDate = DateTime.Now;

        modelBuilder.Entity<UnionPermissionsRole>().HasData(permissions);
      }

      /*
       * Permissions of users
       */
      List<UnionPermissionsUser> permissionsUsers = new List<UnionPermissionsUser>()
            {
                // Role: superuser@mail.com
                new UnionPermissionsUser() { PermissionId = 1, UserId = 1 }, // Permission: Roles
                new UnionPermissionsUser() { PermissionId = 2, UserId = 1 }, // Permission: Users
            };

      foreach (var permissions in permissionsUsers)
      {
        permissions.Status = true;
        permissions.AssignedBy = 1;
        permissions.AssignedDate = DateTime.Now;

        modelBuilder.Entity<UnionPermissionsUser>().HasData(permissions);
      }
    }
  }
}
