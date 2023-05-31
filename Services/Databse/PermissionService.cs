using Starter_NET_7.DTOs.Response.Permission;
using Microsoft.EntityFrameworkCore;
using Starter_NET_7.DTOs.Response.General;
using Starter_NET_7.Database.Models;
using Starter_NET_7.Database;
using Starter_NET_7.Config;

namespace Starter_NET_7.Services.Databse
{
    public class PermissionService
    {
        private readonly AppDbContext _dbContext;
        private readonly AppSettings _appSettings;

        public PermissionService(AppDbContext dbContext, AppSettings appSettings)
        {
            _dbContext = dbContext;
            _appSettings = appSettings;
        }

        public async Task<IEnumerable<Permission>> GetModelPermissionsByIds(int[] permissions)
        {
            return await _dbContext.Permissions.Where(x => x.Status == true && permissions.Contains(x.IdPermission)).ToListAsync();
        }

        public async Task<ICollection<PermissionResponse>> GetAll()
        {
            return await _dbContext.Permissions.Where(x => x.Status == true)
                .OrderBy(x => x.Name)
                .Select(x => new PermissionResponse
                {
                    IdPermission = x.IdPermission,
                    Name = x.Name,
                    Status = x.Status,
                    CreatedBy = x.CreatedBy,
                    CreationDate = x.CreationDate.ToString(_appSettings.DateFormar),
                    LastUpdateBy = x.LastUpdateBy,
                    LastUpdateDate = x.LastUpdateDate.HasValue ? x.LastUpdateDate.Value.ToString(_appSettings.DateFormar) : null
                }).ToListAsync();
        }

        public async Task<ICollection<SelectResponse>> GetAllSelect()
        {
            return await _dbContext.Permissions
                .Where(x => x.Status == true)
                .OrderBy(x => x.Name)
                .Select(x => new SelectResponse
                {
                    Id = x.IdPermission,
                    Name = x.Name,
                })
                .ToListAsync();
        }

        public async Task<PermissionResponse?> GetPermissionById(int id)
        {
            return await _dbContext.Permissions
                .Select(x => new PermissionResponse
                {
                    IdPermission = x.IdPermission,
                    Name = x.Name,
                    Status = x.Status,
                    CreatedBy = x.CreatedBy,
                    CreationDate = x.CreationDate.ToString(_appSettings.DateFormar),
                    LastUpdateBy = x.LastUpdateBy,
                    LastUpdateDate = x.LastUpdateDate.HasValue ? x.LastUpdateDate.Value.ToString(_appSettings.DateFormar) : null
                })
                .FirstOrDefaultAsync(x => x.IdPermission == id);
        }
    }
}
