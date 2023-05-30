using Starter_NET_7.AppSettings;
using Starter_NET_7.DTOs.Response.Permission;
using Microsoft.EntityFrameworkCore;
using Starter_NET_7.DTOs.Response.General;
using Starter_NET_7.Database.Models;
using Starter_NET_7.Database;

namespace Starter_NET_7.Services.Databse
{
    public class PermissionService
    {
        private readonly AppDbContext _dbContext;
        private readonly ConfigApp _configApp;

        public PermissionService(AppDbContext dbContext, ConfigApp configApp)
        {
            _dbContext = dbContext;
            _configApp = configApp;
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
                    CreationDate = x.CreationDate.ToString(_configApp.DateFormar),
                    LastUpdateBy = x.LastUpdateBy,
                    LastUpdateDate = x.LastUpdateDate.HasValue ? x.LastUpdateDate.Value.ToString(_configApp.DateFormar) : null
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
                    CreationDate = x.CreationDate.ToString(_configApp.DateFormar),
                    LastUpdateBy = x.LastUpdateBy,
                    LastUpdateDate = x.LastUpdateDate.HasValue ? x.LastUpdateDate.Value.ToString(_configApp.DateFormar) : null
                })
                .FirstOrDefaultAsync(x => x.IdPermission == id);
        }
    }
}
