using Microsoft.EntityFrameworkCore;
using Starter_NET_7.Database;
using Starter_NET_7.Database.Models;
using System;

namespace Starter_NET_7.Services.Databse
{
    public class UserValidationService
    {
        private readonly AppDbContext _dbContext;

        public UserValidationService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserValidation?> GetModelByUser(int idUser)
        {
            return await _dbContext.UserValidation.FirstAsync(x => x.UserId == idUser);
        }

        public async Task<UserValidation?> GetModelByUuid(string uuid)
        {
            return await _dbContext.UserValidation.FirstOrDefaultAsync(x => x.ForgotPasswordUuid == uuid);
        }

        public async Task SaveRefreshToken(int idUser, string refreshToken, DateTime _expiresRefresh)
        {
            var userValidation = await GetModelByUser(idUser);

            if (userValidation == null)
            {
                userValidation = new UserValidation()
                {
                    UserId = idUser
                };

                _dbContext.UserValidation.Add(userValidation);
                await _dbContext.SaveChangesAsync();
            }

            userValidation!.RefreshToken = refreshToken;
            userValidation.RefreshTokenExpiry = _expiresRefresh;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<string> SaveForgotPassword(int idUser, DateTime expireAt)
        {
            string uuid = Guid.NewGuid().ToString();
            var userValidation = await GetModelByUser(idUser);
            userValidation!.ForgotPasswordUuid = uuid;
            userValidation.ForgotPasswordExpiry = expireAt;
            await _dbContext.SaveChangesAsync();

            return uuid;
        }
    }
}
