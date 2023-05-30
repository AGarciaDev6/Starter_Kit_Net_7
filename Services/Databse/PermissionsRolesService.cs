using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Starter_NET_7.Database;
using Starter_NET_7.Database.Models;
using Starter_NET_7.DTOs.Response.Permission;
using Starter_NET_7.Interfaces;

namespace Starter_NET_7.Services.Databse
{
    public class PermissionsRolesService
    {
        private readonly AppDbContext _dbContext;
        private readonly int _idUser;

        public PermissionsRolesService(AppDbContext dbContext, IToken token)
        {
            _dbContext = dbContext;
            _idUser = token.GetIdUserOfToken();
        }

        public async Task<IEnumerable<PermissionCompactResponse>> GetPermissions(int roleId)
        {
            return await _dbContext.UnionPermissionsRoles
                .Where(x => x.RoleId == roleId && x.Status == true)
                .Select(x => new PermissionCompactResponse
                {
                    IdPermission = x.Permission.IdPermission,
                    Name = x.Permission.Name,
                }).ToListAsync();
        }

        public async Task<ICollection<UnionPermissionsRole>> GetModelPermissionsByIds(int[] permissions, int roleId)
        {
            return await _dbContext.UnionPermissionsRoles.Where(x => x.Status == true && x.RoleId == roleId && permissions.Contains(x.PermissionId)).ToListAsync();
        }

        public async Task<bool> SyncPermissionByRole(int[] permissions, int idRole)
        {
            try
            {
                foreach (var permission in permissions)
                {
                    var oPermission = await _dbContext.UnionPermissionsRoles.FirstOrDefaultAsync(x => x.PermissionId == permission && x.RoleId == idRole);
                    if (oPermission != null)
                    {
                        oPermission.Status = true;
                        oPermission.AssignedBy = _idUser;
                        oPermission.AssignedDate = DateTime.Now;

                        _dbContext.Update(oPermission);
                    }
                    else
                    {
                        oPermission = new UnionPermissionsRole
                        {
                            PermissionId = permission,
                            RoleId = idRole,
                            Status = true,
                            AssignedBy = _idUser,
                            AssignedDate = DateTime.Now,
                        };

                        _dbContext.Add(oPermission);
                    }
                }

                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAllPermissions(int idRole)
        {
            try
            {
                var permissions = await _dbContext.UnionPermissionsRoles.Where(x => x.RoleId == idRole).ToListAsync();
                permissions.ForEach(x =>
                {
                    x.Status = false;
                    x.AssignedBy = _idUser;
                    x.AssignedDate = DateTime.Now;
                });

                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
