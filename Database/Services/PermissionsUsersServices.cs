using Microsoft.EntityFrameworkCore;
using Starter_NET_7.Database.Models;
using Starter_NET_7.Interfaces;

namespace Starter_NET_7.Database.Services
{
    public class PermissionsUsersServices
    {

        private readonly AppDbContext _dbContext;
        private readonly int _idUser;

        public PermissionsUsersServices(AppDbContext dbContext, IToken token)
        {
            _dbContext = dbContext;
            _idUser = token.GetIdUserOfToken();
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
                        oPermission.AssignedBy = _idUser;
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

        public async Task<bool> DeleteAllPermissions(int idUser)
        {
            try
            {
                var permissions = await _dbContext.UnionPermissionsUsers.Where(x => x.UserId == idUser).ToListAsync();
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
