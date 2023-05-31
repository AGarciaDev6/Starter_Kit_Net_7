using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Starter_NET_7.DTOs.Request.AuthController;
using Starter_NET_7.DTOs.Response.Auth;
using Starter_NET_7.DTOs.Response.Role;
using Starter_NET_7.DTOs.Response.User;
using Starter_NET_7.Interfaces;
using System.Net.Mail;
using System.ComponentModel.DataAnnotations;
using Starter_NET_7.Helpers.Validations;
using Starter_NET_7.Database;
using Starter_NET_7.Services.Databse;
using Starter_NET_7.Config;

namespace Starter_NET_7.Controllers.API
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppSettings _appSettings;
        private readonly AppDbContext _dbContext;
        private readonly IToken _token;
        private readonly IEmailSender _emailSender;
        private readonly UserService _userService;
        private readonly UserValidationService _userValidationService;
        private readonly PermissionsUsersServices _permissionsUsersServices;
        private readonly DateTime _expiresToken;
        private readonly DateTime _expiresRefresh;
        private readonly DateTime _expireTokenForgot;

        public AuthController(
            AppDbContext dbContext,
            IToken iToken,
            IEmailSender emailSender,
            UserService userService,
            UserValidationService userValidationService,
            PermissionsUsersServices permissionsUsersServices,
            AppSettings appSettings)
        {
            _appSettings = appSettings;

            _dbContext = dbContext;
            _token = iToken;
            _emailSender = emailSender;
            _userService = userService;
            _userValidationService = userValidationService;
            _permissionsUsersServices = permissionsUsersServices;

            _expiresToken = DateTime.Now.AddHours(_appSettings.ExpireToken);
            _expiresRefresh = DateTime.Now.AddDays(_appSettings.ExpireRefreshToken);
            _expireTokenForgot = DateTime.Now.AddDays(_appSettings.ExpireTokenForgot);
        }

        #region POST /api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            try
            {
                var user = await _userService.GetModelByEmail(request.Email);

                if (user == null)
                {
                    return BadRequest("Email or Password invalid");
                }

                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                {
                    return BadRequest("Email or Password invalid");
                }

                if (user.Status == false)
                {
                    return Unauthorized("Not active in the app");
                }

                string token = _token.CreateToken(user.IdUser, _expiresToken);
                string refreshToken = _token.CreateRefreshToken(_expiresRefresh);
                await _userValidationService.SaveRefreshToken(user.IdUser, refreshToken, _expiresRefresh);
                var permissions = await _permissionsUsersServices.GetPermission(user.IdUser);
                return Ok(new LoginResponse
                {
                    User = new UserCompactResponse
                    {
                        IdUser = user.IdUser,
                        Name = user.Name,
                        LastName = user.LastName,
                        FullName = $"{user.Name} {user.LastName}",
                        Email = user.Email,
                        Role = new RoleCompactResponse
                        {
                            IdRole = user.Role.IdRole,
                            Name = user.Role.Name
                        },
                        Permissions = permissions
                    },
                    Authorization = new AuthorizationResponse
                    {
                        Token = token,
                        ExpiresAt = _expiresToken.ToString(_appSettings.DateFormar),
                        RefreshToken = refreshToken,
                        RefreshExpiresAt = _expiresRefresh.ToString(_appSettings.DateFormar)
                    }
                });
            }
            catch
            {
                return Problem("An error occurred while trying to Login");
            }
        }
        #endregion

        #region Delete /api/auth/logout
        //[Authorize]
        //[HttpDelete("logout")]
        //public async Task<ActionResult> Logout()
        //{
        //    try
        //    {


        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        return Problem();
        //    }
        //}
        #endregion

        #region POST /api/auth/refresh-token
        [Authorize]
        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthorizationResponse>> RefreshToken(RefreshTokenRequest request)
        {
            try
            {
                int idUser = _token.GetIdUserOfToken();

                if (idUser == -1)
                {
                    return BadRequest("Invalid access token");
                }

                var userValidation = await _userValidationService.GetModelByUser(idUser);

                if (userValidation == null || userValidation.RefreshToken != request.RefreshToken || userValidation.RefreshTokenExpiry <= DateTime.Now)
                    return BadRequest("Invalid refresh token");

                string token = _token.CreateToken(idUser, _expiresToken);
                string refreshToken = _token.CreateRefreshToken(_expiresRefresh);
                await _userValidationService.SaveRefreshToken(idUser, refreshToken, _expiresRefresh);

                return Ok(new AuthorizationResponse
                {
                    Token = token,
                    ExpiresAt = _expiresToken.ToString(_appSettings.DateFormar),
                    RefreshToken = refreshToken,
                    RefreshExpiresAt = _expiresRefresh.ToString(_appSettings.DateFormar)
                });
            }
            catch
            {
                return Problem("An error occurred while trying Refresh token");
            }
        }
        #endregion

        #region GET /api/auth/me
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserCompactResponse>> GetMe()
        {
            try
            {
                int idUser = _token.GetIdUserOfToken();

                if (idUser == -1)
                {
                    return BadRequest("Token invalid");
                }

                var user = await _userService.GetModelActiveById(idUser);
                var permissions = await _permissionsUsersServices.GetPermission(idUser);

                return Ok(new UserCompactResponse
                {
                    IdUser = user!.IdUser,
                    Name = user.Name,
                    LastName = user.LastName,
                    FullName = $"{user.Name} {user.LastName}",
                    Email = user.Email,
                    Role = new RoleCompactResponse
                    {
                        IdRole = user.Role.IdRole,
                        Name = user.Role.Name
                    },
                    Permissions = permissions
                });
            }
            catch
            {
                return Problem("An error occurred while trying to query the Profile info");
            }
        }
        #endregion

        #region POST /api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword([FromHeader(Name = "X-Path-Redirect")][Required] string pathRedirect, ForgotPasswordRequest request)
        {
            try
            {
                var user = await _userService.GetModelByEmail(request.Email);

                if (user == null)
                {
                    return Ok("A recovery email was sent");
                }

                string uuid = await _userValidationService.SaveForgotPassword(user.IdUser, _expireTokenForgot);
                string body = await _emailSender.ViewToString("ForgotPassword", new
                    {
                        UserName = $"{user.Name} {user.LastName}",
                        hrefRedirect =  $"{_appSettings.PathFrontend}/{pathRedirect}" 
                    });

                MailMessage mail = _emailSender.CreateSmtpMail("Correo de prueba", user.Email, body, true);

                _emailSender.SendSmtpMail(mail);
                return Ok("A recovery email was sent");
            }
            catch
            {
                return Problem("An error occurred while trying Send Email");
            }
        }
        #endregion

        #region POST /api/auth/reset-password/{uuid}
        [HttpPost("reset-password/{uuid}")]
        public async Task<ActionResult> ResetPassword(string uuid, ResetPasswordRequest request)
        {
            try
            {
                var userValidation = await _userValidationService.GetModelByUuid(uuid);

                if (userValidation == null && userValidation!.ForgotPasswordExpiry < DateTime.Now)
                {
                    return BadRequest("Token expired");
                }

                var user = await _dbContext.Users.FindAsync(userValidation.UserId);

                if (user!.Email.ToLower() != request.Email.Trim().ToLower())
                {
                    return BadRequest("Email not supported");
                }

                if (request.Password.Trim() != request.PasswordConfirm.Trim())
                {
                    return BadRequest("Passwords must match");
                }

                await _userService.UpdatePassword(user, userValidation, request.Password);

                return Ok();
            }
            catch
            {
                return Problem("An error occurred while trying Forgot password");
            }
        }
        #endregion
    }
}
