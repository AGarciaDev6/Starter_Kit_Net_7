using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Starter_NET_7.DTOs.Response.General;
using Starter_NET_7.DTOs.Response.Permission;
using Starter_NET_7.Database.Services;

namespace Starter_NET_7.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/permissions")]
    public class PermissionController : ControllerBase
    {
        private readonly PermissionService _permissionService;

        public PermissionController(PermissionService permissionService)
        {
            this._permissionService = permissionService;
        }

        #region GET /api/permissions/get-all
        [HttpGet("get-all")]
        public async Task<ActionResult<ICollection<PermissionResponse>>> GetAll()
        {
            try
            {
                var permissions = await _permissionService.GetAll();
                return Ok(permissions);
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
                var permissions = await _permissionService.GetAllSelect();

                return Ok(permissions);
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
                var permission = await _permissionService.GetPermissionById(id);

                if (permission == null)
                {
                    return NotFound("The permission was not found");
                }

                return Ok(permission);
            }
            catch
            {
                return Problem("An error occurred while trying to query the Permission");
            }
        }
        #endregion
    }
}
