using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Starter_NET_7.AppSettings;
using Starter_NET_7.Database.Models;
using Starter_NET_7.DTOs.Request.Role;
using Starter_NET_7.DTOs.Response.General;
using Starter_NET_7.DTOs.Response.Role;
using Starter_NET_7.Interfaces;
using System.Data;

namespace Starter_NET_7.Database.Services
{
    public class RoleService
    {
        private readonly AppDbContext _dbContext;
        private readonly int _idUser;

        public RoleService(AppDbContext dbContext, IToken token)
        {
            _dbContext = dbContext;
            _idUser = token.GetIdUserOfToken();
        }

        public async Task<IEnumerable<RoleResponse>> GetAllByStatus(bool status)
        {
            return await _dbContext.Roles
                .Where(x => x.Status == status && x.Name != "Root")
                .OrderBy(x => x.Name)
                .Select(x => new RoleResponse
                {
                    IdRole = x.IdRole,
                    Name = x.Name,
                    Status = x.Status,
                    CreatedBy = x.CreatedBy,
                    CreationDate = x.CreationDate.ToString(ConfigApp.DateFormar),
                    LastUpdateBy = x.LastUpdateBy,
                    LastUpdateDate = x.LastUpdateDate.HasValue ? x.LastUpdateDate.Value.ToString(ConfigApp.DateFormar) : null
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<SelectResponse>> GetAllSelect()
        {
            return await _dbContext.Roles
                .Where(x => x.Status == true && x.Name != "Root")
                .OrderBy(x => x.Name)
                .Select(x => new SelectResponse
                {
                    Id = x.IdRole,
                    Name = x.Name,
                })
                .ToListAsync();
        }

        public async Task<RoleWithPermissionsResponse?> GetRoleById(int id)
        {
            var permissions = await _dbContext.UnionPermissionsRoles.Where(x => x.RoleId == id && x.Status == true).Select(x => x.PermissionId).ToArrayAsync();
            var role = await _dbContext.Roles.Where(x => x.IdRole == id && x.Name != "Root")
                .Select(x => new RoleWithPermissionsResponse
                {
                    IdRole = x.IdRole,
                    Name = x.Name,
                    Status = x.Status,
                    CreatedBy = x.CreatedBy,
                    CreationDate = x.CreationDate.ToString(ConfigApp.DateFormar),
                    LastUpdateBy = x.LastUpdateBy,
                    LastUpdateDate = x.LastUpdateDate.HasValue ? x.LastUpdateDate.Value.ToString(ConfigApp.DateFormar) : null,
                    Permissions = permissions
                })
                .FirstOrDefaultAsync();

            return role;
        }

        public async Task<Role> Create(RoleRequest request)
        {
            var role = new Role()
            {
                Name = request.Name.Trim(),
                Status = true,
                CreatedBy = _idUser,
                CreationDate = DateTime.Now,
            };

            _dbContext.Roles.Add(role);
            await _dbContext.SaveChangesAsync();

            return role;
        }

        public async Task Update(Role role, RoleRequest request)
        {
            role.Name = request.Name.Trim();
            role.LastUpdateBy = _idUser;
            role.LastUpdateDate = DateTime.Now;

            _dbContext.Roles.Update(role);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Role?> GetModelActiveById(int id)
        {
            return await _dbContext.Roles.FirstOrDefaultAsync(x => x.IdRole == id && x.Status == true);
        }

        public async Task<Role?> GetModelById(int id)
        {
            return await _dbContext.Roles.FirstOrDefaultAsync(x => x.IdRole == id);
        }

        public async Task ChangeStatus(Role role, bool status)
        {
            role.Status = status;
            role.LastUpdateBy = _idUser;
            role.LastUpdateDate = DateTime.Now;

            _dbContext.Roles.Update(role);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> ExistById(int id)
        {
            return await _dbContext.Roles.AnyAsync(x => x.IdRole == id);
        }

        public async Task<bool> ExistActiveById(int id)
        {
            return await _dbContext.Roles.AnyAsync(x => x.IdRole == id && x.Status == true);
        }

        public async Task<bool> ExistByName(string name)
        {
            return await _dbContext.Roles.AnyAsync(x => x.Name.ToLower() == name.ToLower());
        }

    }
}
