using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Starter_NET_7.DTOs.Request.AuthController;
using Starter_NET_7.DTOs.Response.Auth;
using Starter_NET_7.DTOs.Response.Permission;
using Starter_NET_7.DTOs.Response.Role;
using Starter_NET_7.DTOs.Response.User;
using Starter_NET_7.Interfaces;
using Starter_NET_7.AppSettings;
using System.Net.Mail;
using System.ComponentModel.DataAnnotations;
using Starter_NET_7.Helpers.Validations;
using Starter_NET_7.Database;

namespace Starter_NET_7.Controllers
{
  [ApiController]
  [Route("api/auth")]
  public class AuthController : ControllerBase
  {
    private readonly AppDbContext _dbContext;
    private readonly IToken _token;
    private readonly IEmailSender _emailSender;
    private DateTime _expires = DateTime.Now.AddHours(2);
    private DateTime _expiresRefresh = DateTime.Now.AddDays(7);

    public AuthController(AppDbContext dbContext, IToken iToken, IEmailSender emailSender)
    {
      this._dbContext = dbContext;
      this._token = iToken;
      this._emailSender = emailSender;
    }

    #region POST /api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
      try
      {
        var oUser = await _dbContext.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Email == request.Email);

        if (oUser == null)
        {
          return BadRequest("Email or Password invalid");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, oUser.Password))
        {
          return BadRequest("Email or Password invalid");
        }

        if (oUser.Status == false)
        {
          return Unauthorized("Not active in the app");
        }

        string token = _token.CreateToken(oUser.IdUser, _expires);
        string refreshToken = _token.CreateRefreshToken(_expiresRefresh);

        var oUserValidation = await _dbContext.UserValidation.FirstAsync(x => x.UserId == oUser.IdUser);

        oUserValidation.RefreshToken = refreshToken;
        oUserValidation.RefreshTokenExpiry = _expiresRefresh;
        await _dbContext.SaveChangesAsync();

        var permissions = await _dbContext.UnionPermissionsUsers.Include(x => x.Permission).Where(x => x.UserId == oUser.IdUser && x.Status == true)
            .Select(x => new PermissionCompactResponse
            {
              IdPermission = x.Permission.IdPermission,
              Name = x.Permission.Name
            }).ToArrayAsync();

        return Ok(new LoginResponse
        {
          User = new UserCompactResponse
          {
            IdUser = oUser.IdUser,
            Name = oUser.Name,
            LastName = oUser.LastName,
            FullName = $"{oUser.Name} {oUser.LastName}",
            Email = oUser.Email,
            Role = new RoleCompactResponse
            {
              IdRole = oUser.Role.IdRole,
              Name = oUser.Role.Name
            },
            Permissions = permissions
          },
          Authorization = new AuthorizationResponse
          {
            Token = token,
            ExpiresAt = _expires.ToString(ConfigApp.DateFormar),
            RefreshToken = refreshToken,
            RefreshExpiresAt = _expiresRefresh.ToString(ConfigApp.DateFormar)
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

        var oUserValidation = await _dbContext.UserValidation.FirstAsync(x => x.UserId == idUser);

        if (oUserValidation == null || oUserValidation.RefreshToken != request.RefreshToken || oUserValidation.RefreshTokenExpiry <= DateTime.Now)
          return BadRequest("Invalid refresh token");

        string token = _token.CreateToken(idUser, _expires);
        string refreshToken = _token.CreateRefreshToken(_expiresRefresh);

        oUserValidation.RefreshToken = refreshToken;
        oUserValidation.RefreshTokenExpiry = _expiresRefresh;
        await _dbContext.SaveChangesAsync();

        return Ok(new AuthorizationResponse
        {
          Token = token,
          ExpiresAt = _expires.ToString(ConfigApp.DateFormar),
          RefreshToken = refreshToken,
          RefreshExpiresAt = _expiresRefresh.ToString(ConfigApp.DateFormar)
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

        var oUser = await _dbContext.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.IdUser == idUser);

        var permissions = await _dbContext.UnionPermissionsUsers.Include(x => x.Permission).Where(x => x.UserId == oUser!.IdUser && x.Status == true)
            .Select(x => new PermissionCompactResponse
            {
              IdPermission = x.Permission.IdPermission,
              Name = x.Permission.Name,
            }).ToListAsync();

        return Ok(new UserCompactResponse
        {
          IdUser = oUser!.IdUser,
          Name = oUser.Name,
          LastName = oUser.LastName,
          FullName = $"{oUser.Name} {oUser.LastName}",
          Email = oUser.Email,
          Role = new RoleCompactResponse
          {
            IdRole = oUser.Role.IdRole,
            Name = oUser.Role.Name
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
    public async Task<ActionResult> ForgotPassword([FromHeader(Name = "X-Url-Redirect")][Required] string urlRedirect, ForgotPasswordRequest request)
    {
      try
      {
        if (!UrlValidation.IsValidUrl(urlRedirect))
        {
          return BadRequest("The header X-Url-Redirect not is valid");
        }

        var oUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == request.Email);

        if (oUser == null)
        {
          return Ok("A recovery email was sent");
        }

        string uuid = Guid.NewGuid().ToString();
        var oUserValidation = await _dbContext.UserValidation.FirstAsync(x => x.UserId == oUser.IdUser);

        var expireAt = DateTime.Now.AddDays(2);

        oUserValidation.ForgotPasswordUuid = uuid;
        oUserValidation.ForgotPasswordExpiry = expireAt;

        await _dbContext.SaveChangesAsync();

        IEnumerable<string> addresses = new List<string>() { oUser.Email };
        MailMessage mail = _emailSender.CreateSmtpMail("Correo de prueba", addresses, $"{uuid} <br> {expireAt}", true);

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
        var oUserValidation = await _dbContext.UserValidation.FirstOrDefaultAsync(x => x.ForgotPasswordUuid == uuid);

        if (oUserValidation == null && oUserValidation!.ForgotPasswordExpiry < DateTime.Now)
        {
          return BadRequest("Token expired");
        }

        var oUser = await _dbContext.Users.FindAsync(oUserValidation.UserId);

        if (oUser!.Email.ToLower() != request.Email.Trim().ToLower())
        {
          return BadRequest("Email not supported");
        }

        if (request.Password.Trim() != request.PasswordConfirm.Trim())
        {
          return BadRequest("Passwords must match");
        }

        oUserValidation.ForgotPasswordUuid = null;
        oUserValidation.ForgotPasswordExpiry = null;
        _dbContext.Update(oUserValidation);

        oUser.Password = BCrypt.Net.BCrypt.HashPassword(request.Password.Trim());
        oUser.LastUpdateBy = oUser.IdUser;
        oUser.LastUpdateDate = DateTime.Now;
        _dbContext.Update(oUser);

        await _dbContext.SaveChangesAsync();

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
