using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Starter_NET_7.DTOs.Request.Role;
using Starter_NET_7.DTOs.Response.General;
using Starter_NET_7.DTOs.Response.Role;
using Starter_NET_7.Database.Models;
using Starter_NET_7.AppSettings;
using Starter_NET_7.DTOs.Response.Permission;
using Starter_NET_7.Database;

namespace Starter_NET_7.Controllers
{
  [Authorize]
  [ApiController]
  [Route("api/roles")]
  public class RolesController : ControllerBase
  {
    private readonly AppDbContext _dbContext;

    public RolesController(AppDbContext dbContext)
    {
      this._dbContext = dbContext;
    }

    #region GET /api/roles/get-all/{status}
    [HttpGet("get-all/{status:bool}")]
    //[HasPermission("Roles")]
    public async Task<ActionResult<IEnumerable<RoleResponse>>> GetAllByStatus(bool status)
    {
      try
      {
        var roles = await _dbContext.Roles.Include(x => x.UnionPermissionsRoles).Where(x => x.Status == status && x.Name != "Root").OrderBy(x => x.Name)
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

        return Ok(roles);
      }
      catch
      {
        return Problem("An error occurred while trying to query the Roles");
      }
    }
    #endregion

    #region GET /api/roles/get-all/select
    [HttpGet("get-all/select")]
    public async Task<ActionResult<IEnumerable<SelectResponse>>> GetAllSelect()
    {
      try
      {
        var roles = await _dbContext.Roles.Where(x => x.Status == true && x.Name != "Root").OrderBy(x => x.Name)
        .Select(x => new SelectResponse
        {
          Id = x.IdRole,
          Name = x.Name,
        })
        .ToListAsync();

        return Ok(roles);
      }
      catch
      {
        return Problem("An error occurred while trying to query the Roles");
      }
    }
    #endregion

    #region GET /api/roles/{id}
    [HttpGet("{id:int}")]
    //[HasPermission("Roles")]
    public async Task<ActionResult<RoleWithPermissionsResponse>> GetById(int id)
    {
      try
      {
        var permissions = await _dbContext.UnionPermissionsRoles.Where(x => x.RoleId == id && x.Status == true).Select(x => x.PermissionId).ToArrayAsync();
        var oRole = await _dbContext.Roles.Where(x => x.IdRole == id && x.Name != "Root")
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

        if (oRole == null)
        {
          return NotFound("The Role was not found");
        }

        return Ok(oRole);
      }
      catch
      {
        return Problem("An error occurred while trying to query the Role");
      }
    }
    #endregion

    #region GET /api/roles/{id}/permissions
    [HttpGet("{id:int}/permissions")]
    //[HasPermission("Roles")]
    public async Task<ActionResult<PermissionCompactResponse>> GetPermissionsByIdRole(int id)
    {
      try
      {
        if (!RoleExistById(id))
        {
          return NotFound("The Role was not found");
        }

        var permissions = await _dbContext.UnionPermissionsRoles.Include(x => x.Permission).Where(x => x.RoleId == id && x.Status == true)
            .Select(x => new PermissionCompactResponse
            {
              IdPermission = x.Permission.IdPermission,
              Name = x.Permission.Name,
            }).ToListAsync();

        return Ok(permissions);
      }
      catch
      {
        return Problem("An error occurring while to query the Permissions of Role");
      }
    }
    #endregion

    #region POST /api/roles
    [HttpPost]
    //[HasPermission("Roles")]
    public async Task<ActionResult<RoleWithPermissionsResponse>> Create(RoleRequest request)
    {
      try
      {
        if (RoleExistByName(request.Name))
        {
          return BadRequest("There is already a Role with the name: " + request.Name);
        }

        var countPermissions = _dbContext.Permissions.Where(x => x.Status == true && request.Permissions.Contains(x.IdPermission)).Count();

        if (countPermissions != request.Permissions.Count())
        {
          return BadRequest("One or more Permissions not exist");
        }

        var role = new Role()
        {
          Name = request.Name.Trim(),
          Status = true,
          CreatedBy = 0,
          CreationDate = DateTime.Now,
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        var successSync = await PermissionSync(request.Permissions, role.IdRole);
        if (!successSync)
        {
          throw new Exception("Not Permission sync");
        }

        var oRole = new RoleWithPermissionsResponse
        {
          IdRole = role.IdRole,
          Name = role.Name,
          Status = role.Status,
          CreatedBy = role.CreatedBy,
          CreationDate = role.CreationDate.ToString(ConfigApp.DateFormar),
          LastUpdateBy = role.LastUpdateBy,
          LastUpdateDate = role.LastUpdateDate.HasValue ? role.LastUpdateDate.Value.ToString(ConfigApp.DateFormar) : null,
          Permissions = request.Permissions
        };

        return CreatedAtAction(nameof(GetById), new { Id = role.IdRole }, oRole);
      }
      catch
      {
        return Problem("An error occurred while trying to create the Role");
      }
    }
    #endregion

    #region PUT /api/roles/{id}
    [HttpPut("{id:int}")]
    //[HasPermission("Roles")]
    public async Task<ActionResult> Update(int id, RoleRequest request)
    {
      try
      {
        var oRole = await _dbContext.Roles.FirstOrDefaultAsync(x => x.IdRole == id && x.Status == true);

        if (oRole == null)
        {
          return NotFound("The Role was not found");
        }

        if (RoleExistByName(request.Name) && request.Name.Trim().ToLower() != oRole.Name.ToLower())
        {
          return BadRequest("There is already a Role with the name: " + request.Name);
        }

        var countPermissions = _dbContext.Permissions.Where(x => x.Status == true && request.Permissions.Contains(x.IdPermission)).Count();

        if (countPermissions != request.Permissions.Count())
        {
          return BadRequest("One or more Permissions not exist");
        }

        var successDelete = await DeleteAllPermissions(id);
        if (!successDelete)
        {
          throw new Exception("Not delete Permissions");
        }

        var successSync = await PermissionSync(request.Permissions, id);
        if (!successSync)
        {
          throw new Exception("Not Permission sync");
        }

        oRole.Name = request.Name.Trim();
        oRole.LastUpdateBy = 0;
        oRole.LastUpdateDate = DateTime.Now;

        _dbContext.Roles.Update(oRole);
        await _dbContext.SaveChangesAsync();

        return NoContent();
      }
      catch
      {
        return Problem("An error occurred while trying to update the Role");
      }
    }
    #endregion

    #region DELETE api/roles/{id}
    [HttpDelete("{id:int}")]
    //[HasPermission("Roles")]
    public async Task<ActionResult> Delete(int id)
    {
      try
      {
        var oRole = await _dbContext.Roles.FirstOrDefaultAsync(x => x.IdRole == id);

        if (oRole == null)
        {
          return NotFound("The Role was not found");
        }

        oRole.Status = false;
        oRole.LastUpdateBy = 0;
        oRole.LastUpdateDate = DateTime.Now;

        _dbContext.Roles.Update(oRole);
        await _dbContext.SaveChangesAsync();

        return Ok();
      }
      catch
      {
        return Problem("An error occurred while trying to delete the Role");
      }
    }
    #endregion+

    #region POST api/roles/{id}/reactive
    [HttpPost("{id:int}/reactive")]
    //[HasPermission("Roles")]
    public async Task<ActionResult> Reactivate(int id)
    {
      try
      {
        var oRole = await _dbContext.Roles.FirstOrDefaultAsync(x => x.IdRole == id);

        if (oRole == null)
        {
          return NotFound("The Role was not found");
        }

        oRole.Status = true;
        oRole.LastUpdateBy = 0;
        oRole.LastUpdateDate = DateTime.Now;

        _dbContext.Roles.Update(oRole);
        await _dbContext.SaveChangesAsync();

        return Ok();
      }
      catch
      {
        return Problem("An error occurred while trying to reactive the Role");
      }
    }
    #endregion




    /**
     * Method privates of this class
     */

    private bool RoleExistById(int id)
    {
      return _dbContext.Roles.Any(x => x.IdRole == id);
    }

    private bool RoleExistByName(string name)
    {
      return _dbContext.Roles.Any(x => x.Name.ToLower() == name.ToLower());
    }

    private async Task<bool> PermissionSync(int[] permissions, int idRole)
    {
      try
      {
        foreach (var permission in permissions)
        {
          var oPermission = await _dbContext.UnionPermissionsRoles.FirstOrDefaultAsync(x => x.PermissionId == permission && x.RoleId == idRole);
          if (oPermission != null)
          {
            oPermission.Status = true;
            oPermission.AssignedBy = 0;
            oPermission.AssignedDate = DateTime.Now;

            _dbContext.Update(oPermission);
          }
          else
          {
            oPermission = new UnionPermissionsRole
            {
              PermissionId = permission,
              RoleId = idRole,
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

    private async Task<bool> DeleteAllPermissions(int idRole)
    {
      try
      {
        var permissions = await _dbContext.UnionPermissionsRoles.Where(x => x.RoleId == idRole).ToListAsync();
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
