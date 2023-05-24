using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Starter_NET_7.DTOs.Response.General;
using Starter_NET_7.DTOs.Response.Permission;
using Starter_NET_7.AppSettings;
using System.Collections;
using Starter_NET_7.Database;

namespace Starter_NET_7.Controllers
{
    [Authorize]
    [ApiController]
  [Route("api/permissions")]
  public class PermissionController : ControllerBase
  {
    private readonly AppDbContext _dbContext;

    public PermissionController(AppDbContext dbContext)
    {
      this._dbContext = dbContext;
    }

    #region GET /api/permissions/get-all
    [HttpGet("get-all")]
    public async Task<ActionResult<ICollection<PermissionResponse>>> GetAll()
    {
      try
      {
        var oPermissions = await _dbContext.Permissions.Where(x => x.Status == true).OrderBy(x => x.Name).Select(x => new PermissionResponse
        {
          IdPermission = x.IdPermission,
          Name = x.Name,
          Status = x.Status,
          CreatedBy = x.CreatedBy,
          CreationDate = x.CreationDate.ToString(ConfigApp.DateFormar),
          LastUpdateBy = x.LastUpdateBy,
          LastUpdateDate = x.LastUpdateDate.HasValue ? x.LastUpdateDate.Value.ToString(ConfigApp.DateFormar) : null
        }).ToListAsync();

        return Ok(oPermissions);
      }
      catch
      {
        return Problem("An error occurred while trying to query the Permissions");
      }
    }
    #endregion

    #region GET api/permissions/get-all/select
    [HttpGet("get-all/select")]
    public async Task<ActionResult<ICollection<SelectResponse>>> GetAllSelect()
    {
      try
      {
        var oPermissions = await _dbContext.Permissions.Where(x => x.Status == true).OrderBy(x => x.Name).Select(x => new SelectResponse
        {
          Id = x.IdPermission,
          Name = x.Name,
        }).ToListAsync();

        return Ok(oPermissions);
      }
      catch
      {
        return Problem("An error occurred while trying to query the Permissions");
      }
    }
    #endregion

    #region GET api/permissions/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PermissionResponse>> GetById(int id)
    {
      try
      {
        var oPermission = await _dbContext.Permissions.Select(x => new PermissionResponse
        {
          IdPermission = x.IdPermission,
          Name = x.Name,
          Status = x.Status,
          CreatedBy = x.CreatedBy,
          CreationDate = x.CreationDate.ToString(ConfigApp.DateFormar),
          LastUpdateBy = x.LastUpdateBy,
          LastUpdateDate = x.LastUpdateDate.HasValue ? x.LastUpdateDate.Value.ToString(ConfigApp.DateFormar) : null
        }).FirstOrDefaultAsync();

        if (oPermission == null)
        {
          return NotFound("The permission was not found");
        }

        return Ok(oPermission);
      }
      catch
      {
        return Problem("An error occurred while trying to query the Permission");
      }
    }
    #endregion
  }
}
