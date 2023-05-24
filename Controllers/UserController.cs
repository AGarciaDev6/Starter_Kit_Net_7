using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Starter_NET_7.DTOs.Request.User;
using Starter_NET_7.DTOs.Response.General;
using Starter_NET_7.DTOs.Response.Role;
using Starter_NET_7.DTOs.Response.User;
using Starter_NET_7.Filter;
using Starter_NET_7.Database.Models;
using Starter_NET_7.AppSettings;
using System.Collections;
using System.Data;
using Starter_NET_7.Database;

namespace Starter_NET_7.Controllers
{
    [Authorize]
    [ApiController]
  [Route("api/users")]
  public class UserController : ControllerBase
  {
    private readonly AppDbContext _dbContext;

    public UserController(AppDbContext dbContext)
    {
      this._dbContext = dbContext;
    }

    #region GET /api/users/get-all/{status:bool}
    [HttpGet("get-all/{status:bool}")]
    //[HasPermission("Users")]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllByStatus(bool status)
    {
      try
      {
        var users = await _dbContext.Users.Where(x => x.Status == status && x.IdUser != 1).OrderBy(x => x.Name)
            .Select(x => new UserResponse
            {
              IdUser = x.IdUser,
              Name = x.Name,
              LastName = x.LastName,
              Email = x.Email,
              Status = x.Status,
              CreatedBy = x.CreatedBy,
              CreationDate = x.CreationDate.ToString(ConfigApp.DateFormar),
              LastUpdateBy = x.LastUpdateBy,
              LastUpdateDate = x.LastUpdateDate.HasValue ? x.LastUpdateDate.Value.ToString(ConfigApp.DateFormar) : null,
              Role = new RoleCompactResponse
              {
                IdRole = x.Role.IdRole,
                Name = x.Role.Name
              }
            })
            .ToListAsync();

        return Ok(users);
      }
      catch
      {
        return Problem("An error occurred while trying to query the Users");
      }
    }
    #endregion

    #region GET /api/users/get-all/select
    [HttpGet("get-all/select")]
    public async Task<ActionResult<IEnumerable<SelectResponse>>> GetAllSelect()
    {
      try
      {
        var users = await _dbContext.Users.Where(x => x.Status == true && x.IdUser != 1).OrderBy(x => x.Name)
        .Select(x => new SelectResponse
        {
          Id = x.IdUser,
          Name = x.Name,
        })
        .ToListAsync();

        return Ok(users);
      }
      catch
      {
        return Problem("An error occurred while trying to query the Users");
      }
    }
    #endregion

    #region GET /api/users/{id}
    [HttpGet("{id:int}")]
    //[HasPermission("Users")]
    public async Task<ActionResult<UserWithPermissionsResponse>> GetById(int id)
    {
      try
      {
        var permissions = await _dbContext.UnionPermissionsUsers.Where(x => x.UserId == id && x.Status == true).Select(x => x.PermissionId).ToArrayAsync();
        var oUser = await _dbContext.Users.Where(x => x.IdUser == id && x.IdUser != 1)
            .Select(x => new UserWithPermissionsResponse
            {
              IdUser = x.IdUser,
              Name = x.Name,
              LastName = x.LastName,
              Email = x.Email,
              Status = x.Status,
              CreatedBy = x.CreatedBy,
              CreationDate = x.CreationDate.ToString(ConfigApp.DateFormar),
              LastUpdateBy = x.LastUpdateBy,
              LastUpdateDate = x.LastUpdateDate.HasValue ? x.LastUpdateDate.Value.ToString(ConfigApp.DateFormar) : null,
              Role = new RoleCompactResponse
              {
                IdRole = x.Role.IdRole,
                Name = x.Role.Name
              },
              Permissions = permissions
            })
            .FirstOrDefaultAsync();

        if (oUser == null)
        {
          return NotFound("The User was not found");
        }

        return Ok(oUser);
      }
      catch
      {
        return Problem("An error occurred while trying to query the User");
      }
    }
    #endregion

    #region POST /api/users
    [HttpPost]
    //[HasPermission("Users")]
    public async Task<ActionResult> Create(UserCreateRequest request)
    {
      try
      {
        if (UserExistByEmail(request.Email))
        {
          return BadRequest("There is already a User with the Email: " + request.Email);
        }

        if (!RoleExistById(request.IdRole))
        {
          return BadRequest("The Role was not found");
        }

        if (request.Password.Trim() != request.PasswordConfirm.Trim())
        {
          return BadRequest("Passwords must match");
        }

        var countPermissions = _dbContext.UnionPermissionsRoles.Where(x => x.Status == true && x.RoleId == request.IdRole && request.Permissions.Contains(x.PermissionId)).Count();

        if (countPermissions != request.Permissions.Count())
        {
          return BadRequest("One or more Permissions not exists in the Role");
        }

        var user = new User()
        {
          Name = request.Name.Trim(),
          LastName = request.LastName.Trim(),
          Email = request.Email.Trim(),
          Password = BCrypt.Net.BCrypt.HashPassword(request.Password.Trim()),
          Status = true,
          CreatedBy = 0,
          CreationDate = DateTime.Now,
          RoleId = request.IdRole,
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var oUserValidation = new UserValidation()
        {
          UserId = user.IdUser
        };

        _dbContext.UserValidation.Add(oUserValidation);
        await _dbContext.SaveChangesAsync();

        var successSync = await PermissionSync(request.Permissions, user.IdUser);
        if (!successSync)
        {
          throw new Exception("Not Permission Sync");
        }

        var role = await _dbContext.Roles.FindAsync(request.IdRole);

        var oUser = new UserWithPermissionsResponse
        {
          IdUser = user.IdUser,
          Name = user.Name,
          LastName = user.LastName,
          Email = user.Email,
          Status = user.Status,
          CreatedBy = user.CreatedBy,
          CreationDate = user.CreationDate.ToString(ConfigApp.DateFormar),
          LastUpdateBy = user.LastUpdateBy,
          LastUpdateDate = user.LastUpdateDate.HasValue ? user.LastUpdateDate.Value.ToString(ConfigApp.DateFormar) : null,
          Role = new RoleCompactResponse
          {
            IdRole = role!.IdRole,
            Name = role.Name
          },
          Permissions = request.Permissions
        };

        return CreatedAtAction(nameof(GetById), new { Id = user.IdUser }, oUser);
      }
      catch
      {
        return Problem("An error occurred while trying to create the User");
      }
    }
    #endregion

    #region PUT /api/users/{id}
    [HttpPut("{id:int}")]
    //[HasPermission("Users")]
    public async Task<ActionResult> Update(int id, UserUpdateRequest request)
    {
      try
      {
        var oUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.IdUser == id && x.Status == true);
        if (oUser == null)
        {
          return NotFound("The User was not found");
        }

        if (UserExistByEmail(request.Email) && request.Email.Trim().ToLower() != oUser.Email.Trim().ToLower())
        {
          return BadRequest("There is already a User with the Email: " + request.Email);
        }

        if (!RoleExistById(request.IdRole))
        {
          return BadRequest("The Role was not found");
        }

        if (request?.Password?.Trim() != request?.PasswordConfirm?.Trim())
        {
          return BadRequest("Passwords must match");
        }

        var countPermissions = _dbContext.UnionPermissionsRoles.Where(x => x.Status == true && x.RoleId == request!.IdRole && request.Permissions.Contains(x.PermissionId)).Count();

        if (countPermissions != request?.Permissions.Count())
        {
          return BadRequest("One or more Permissions not exists in the Role");
        }

        var successDelete = await DeleteAllPermissions(id);
        if (!successDelete)
        {
          throw new Exception("Not delete Permissions");
        }

        var successSync = await PermissionSync(request.Permissions, id);
        if (!successSync)
        {
          throw new Exception("Not Permission Sync");
        }

        oUser.Name = request.Name.Trim();
        oUser.LastName = request.LastName.Trim();
        oUser.Email = request.Email.Trim();
        oUser.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        oUser.RoleId = request.IdRole;
        oUser.LastUpdateBy = 0;
        oUser.LastUpdateDate = DateTime.Now;

        _dbContext.Users.Update(oUser);
        await _dbContext.SaveChangesAsync();

        return NoContent();
      }
      catch
      {
        return Problem("An error occurred while trying to update the User");
      }
    }
    #endregion

    #region DELETE api/users/{id}
    [HttpDelete("{id:int}")]
    //[HasPermission("Users")]
    public async Task<ActionResult> Delete(int id)
    {
      try
      {
        var oUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.IdUser == id);

        if (oUser == null)
        {
          return NotFound("The User was not found");
        }

        oUser.Status = false;
        oUser.LastUpdateBy = 0;
        oUser.LastUpdateDate = DateTime.Now;

        _dbContext.Users.Update(oUser);
        await _dbContext.SaveChangesAsync();

        return Ok();
      }
      catch
      {
        return Problem("An error occurred while trying to delete the User");
      }
    }
    #endregion

    #region POST api/users/{id}/reactive
    [HttpPost("{id:int}/reactive")]
    //[HasPermission("Users")]
    public async Task<ActionResult> Reactivate(int id)
    {
      try
      {
        var oUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.IdUser == id);

        if (oUser == null)
        {
          return NotFound("The User was not found");
        }

        oUser.Status = true;
        oUser.LastUpdateBy = 0;
        oUser.LastUpdateDate = DateTime.Now;

        _dbContext.Users.Update(oUser);
        await _dbContext.SaveChangesAsync();

        return Ok();
      }
      catch
      {
        return Problem("An error occurred while trying to reactive the User");
      }
    }
    #endregion



    /**
     * Method privates of this class
     */

    private bool RoleExistById(int id)
    {
      return _dbContext.Roles.Any(x => x.IdRole == id && x.Status == true);
    }

    private bool UserExistByEmail(string email)
    {
      return _dbContext.Users.Any(x => x.Email.Trim().ToLower() == email.Trim().ToLower());
    }

    private async Task<bool> PermissionSync(int[] permissions, int idUser)
    {
      try
      {
        foreach (var permission in permissions)
        {
          var oPermission = await _dbContext.UnionPermissionsUsers.FirstOrDefaultAsync(x => x.PermissionId == permission && x.UserId == idUser);
          if (oPermission != null)
          {
            oPermission.Status = true;
            oPermission.AssignedBy = 0;
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
              AssignedBy = 0,
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

    private async Task<bool> DeleteAllPermissions(int idUser)
    {
      try
      {
        var permissions = await _dbContext.UnionPermissionsUsers.Where(x => x.UserId == idUser).ToListAsync();
        permissions.ForEach(x =>
        {
          x.Status = false;
          x.AssignedBy = 0;
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
