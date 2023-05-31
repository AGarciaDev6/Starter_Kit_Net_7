using Microsoft.EntityFrameworkCore;
using Starter_NET_7.Config;
using Starter_NET_7.Database;
using Starter_NET_7.Database.Models;
using Starter_NET_7.DTOs.Request.Profile;
using Starter_NET_7.DTOs.Request.User;
using Starter_NET_7.DTOs.Response.General;
using Starter_NET_7.DTOs.Response.Role;
using Starter_NET_7.DTOs.Response.User;
using Starter_NET_7.Interfaces;

namespace Starter_NET_7.Services.Databse
{
    public class UserService
    {
        private readonly AppDbContext _dbContext;
        private readonly AppSettings _appSettings;
        private readonly IToken _token;

        public UserService(AppDbContext dbContext, AppSettings appSettings, IToken token)
        {
            _dbContext = dbContext;
            _appSettings = appSettings;
            _token = token;
        }

        public async Task<User?> GetModelActiveById(int id)
        {
            return await _dbContext.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.IdUser == id && x.Status == true);
        }

        public async Task<User?> GetModelById(int id)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(x => x.IdUser == id);
        }

        public async Task<User?> GetModelByEmail(string email)
        {
            return await _dbContext.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<bool> ExistByEmail(string email)
        {
            return await _dbContext.Users.AnyAsync(x => x.Email.ToLower() == email.Trim().ToLower());
        }

        public async Task<IEnumerable<UserResponse>> GetAllByStatus(bool status)
        {
            return await _dbContext.Users
                .Where(x => x.Status == status && x.IdUser != 1)
                .OrderBy(x => x.Name)
                .Select(x => new UserResponse
                {
                    IdUser = x.IdUser,
                    Name = x.Name,
                    LastName = x.LastName,
                    Email = x.Email,
                    Status = x.Status,
                    CreatedBy = x.CreatedBy,
                    CreationDate = x.CreationDate.ToString(_appSettings.DateFormar),
                    LastUpdateBy = x.LastUpdateBy,
                    LastUpdateDate = x.LastUpdateDate.HasValue ? x.LastUpdateDate.Value.ToString(_appSettings.DateFormar) : null,
                    Role = new RoleCompactResponse
                    {
                        IdRole = x.Role.IdRole,
                        Name = x.Role.Name
                    }
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<SelectResponse>> GetAllSelect()
        {
            return await _dbContext.Users
                .Where(x => x.Status == true && x.IdUser != 1)
                .OrderBy(x => x.Name)
                .Select(x => new SelectResponse
                {
                    Id = x.IdUser,
                    Name = x.Name,
                })
                .ToListAsync();
        }

        public async Task<UserResponse?> GetById(int id)
        {
            var user = await _dbContext.Users.Where(x => x.IdUser == id && x.IdUser != 1)
                .Select(x => new UserResponse
                {
                    IdUser = x.IdUser,
                    Name = x.Name,
                    LastName = x.LastName,
                    Email = x.Email,
                    Status = x.Status,
                    CreatedBy = x.CreatedBy,
                    CreationDate = x.CreationDate.ToString(_appSettings.DateFormar),
                    LastUpdateBy = x.LastUpdateBy,
                    LastUpdateDate = x.LastUpdateDate.HasValue ? x.LastUpdateDate.Value.ToString(_appSettings.DateFormar) : null,
                    Role = new RoleCompactResponse
                    {
                        IdRole = x.Role.IdRole,
                        Name = x.Role.Name
                    },
                })
                .FirstOrDefaultAsync();

            return user;
        }

        public async Task<UserWithPermissionsResponse?> GetByIdWhitPermission(int id)
        {
            var permissions = await _dbContext.UnionPermissionsUsers.Where(x => x.UserId == id && x.Status == true).Select(x => x.PermissionId).ToArrayAsync();
            var user = await _dbContext.Users.Where(x => x.IdUser == id && x.IdUser != 1)
                .Select(x => new UserWithPermissionsResponse
                {
                    IdUser = x.IdUser,
                    Name = x.Name,
                    LastName = x.LastName,
                    Email = x.Email,
                    Status = x.Status,
                    CreatedBy = x.CreatedBy,
                    CreationDate = x.CreationDate.ToString(_appSettings.DateFormar),
                    LastUpdateBy = x.LastUpdateBy,
                    LastUpdateDate = x.LastUpdateDate.HasValue ? x.LastUpdateDate.Value.ToString(_appSettings.DateFormar) : null,
                    Role = new RoleCompactResponse
                    {
                        IdRole = x.Role.IdRole,
                        Name = x.Role.Name
                    },
                    Permissions = permissions
                })
                .FirstOrDefaultAsync();

            return user;
        }

        public async Task<User> Create(UserCreateRequest request)
        {
            var user = new User()
            {
                Name = request.Name.Trim(),
                LastName = request.LastName.Trim(),
                Email = request.Email.Trim(),
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password.Trim()),
                Status = true,
                CreatedBy = _token.GetIdUserOfToken(),
                CreationDate = DateTime.Now,
                RoleId = request.IdRole,
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            var userValidation = new UserValidation()
            {
                UserId = user.IdUser
            };

            _dbContext.UserValidation.Add(userValidation);
            await _dbContext.SaveChangesAsync();

            return user;
        }

        public async Task Update(User user, UserUpdateRequest request)
        {
            user.Name = request.Name.Trim();
            user.LastName = request.LastName.Trim();
            user.Email = request.Email.Trim();
            user.RoleId = request.IdRole;
            user.LastUpdateBy = _token.GetIdUserOfToken();
            user.LastUpdateDate = DateTime.Now;

            if (request.Password != null)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateStatus(User user, bool status)
        {
            user.Status = status;
            user.LastUpdateBy = _token.GetIdUserOfToken();
            user.LastUpdateDate = DateTime.Now;

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateProfile(User user, ProfileRequest request)
        {
            user.Name = request.Name.Trim();
            user.LastName = request.LastName.Trim();
            user.Email = request.Email.Trim();
            user.LastUpdateBy = _token.GetIdUserOfToken();
            user.LastUpdateDate = DateTime.Now;

            if (request.Password != null)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdatePassword(User user, UserValidation userValidation, string password)
        {
            userValidation.ForgotPasswordUuid = null;
            userValidation.ForgotPasswordExpiry = null;
            _dbContext.Update(userValidation);

            user.Password = BCrypt.Net.BCrypt.HashPassword(password.Trim());
            user.LastUpdateBy = user.IdUser;
            user.LastUpdateDate = DateTime.Now;
            _dbContext.Update(user);

            await _dbContext.SaveChangesAsync();
        }
    }
}
