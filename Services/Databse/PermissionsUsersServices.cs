using Microsoft.EntityFrameworkCore;
using Starter_NET_7.Database;
using Starter_NET_7.Database.Models;
using Starter_NET_7.DTOs.Response.Permission;
using Starter_NET_7.Interfaces;

namespace Starter_NET_7.Services.Databse
{
    public class PermissionsUsersServices
    {

        private readonly AppDbContext _dbContext;
        private readonly IToken _token;

        public PermissionsUsersServices(AppDbContext dbContext, IToken token)
        {
            _dbContext = dbContext;
            _token = token;
        }

        public async Task<ICollection<PermissionCompactResponse>> GetPermission(int idUser)
        {
            return await _dbContext.UnionPermissionsUsers.Include(x => x.Permission).Where(x => x.UserId == idUser && x.Status == true)
                    .Select(x => new PermissionCompactResponse
                    {
                        IdPermission = x.Permission.IdPermission,
                        Name = x.Permission.Name
                    }).ToArrayAsync();
        }

        public async Task<bool> SyncPermission(int[] permissions, int idUser)
        {
            try
            {
                foreach (var permission in permissions)
                {
                    var oPermission = await _dbContext.UnionPermissionsUsers.FirstOrDefaultAsync(x => x.PermissionId == permission && x.UserId == idUser);
                    if (oPermission != null)
                    {
                        oPermission.Status = true;
                        oPermission.AssignedBy = _token.GetIdUserOfToken();
                        oPermission.AssignedDate = DateTime.Now;

                        _dbContext.Update(oPermission);
                    }
                    else
                    {
                        oPermission = new UnionPermissionsUser
                        {
                            PermissionId = permission,
                            UserId = idUser,
                            Status = true,
                            AssignedBy = _token.GetIdUserOfToken(),
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

        public async Task<bool> DeleteAllPermissions(int idUser)
        {
            try
            {
                var permissions = await _dbContext.UnionPermissionsUsers.Where(x => x.UserId == idUser).ToListAsync();
                permissions.ForEach(x =>
                {
                    x.Status = false;
                    x.AssignedBy = _token.GetIdUserOfToken();
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
