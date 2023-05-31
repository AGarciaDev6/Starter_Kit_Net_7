using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Starter_NET_7.DTOs.Request.Role;
using Starter_NET_7.DTOs.Response.General;
using Starter_NET_7.DTOs.Response.Role;
using Starter_NET_7.DTOs.Response.Permission;
using Starter_NET_7.Database;
using Starter_NET_7.Filter;
using Starter_NET_7.Services.Databse;

namespace Starter_NET_7.Controllers.API
{
    [Authorize]
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly RoleService _roleService;
        private readonly PermissionsRolesService _permissionsRoleService;
        private readonly PermissionService _permissionService;

        public RolesController(
            AppDbContext dbContext,
            RoleService roleService,
            PermissionsRolesService permissionsRoleService,
            PermissionService permissionService)
        {
            _dbContext = dbContext;
            _roleService = roleService;
            _permissionsRoleService = permissionsRoleService;
            _permissionService = permissionService;
        }

        #region GET /api/roles/get-all/{status}
        [HttpGet("get-all/{status:bool}")]
        [HasPermission("Roles")]
        public async Task<ActionResult<IEnumerable<RoleResponse>>> GetAllByStatus(bool status)
        {
            try
            {
                var roles = await _roleService.GetAllByStatus(status);
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
                var roles = await _roleService.GetAllSelect();
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
        [HasPermission("Roles")]
        public async Task<ActionResult<RoleWithPermissionsResponse>> GetById(int id)
        {
            try
            {
                var role = await _roleService.GetRoleById(id);

                if (role == null)
                {
                    return NotFound("The Role was not found");
                }

                return Ok(role);
            }
            catch
            {
                return Problem("An error occurred while trying to query the Role");
            }
        }
        #endregion

        #region GET /api/roles/{id}/permissions
        [HttpGet("{id:int}/permissions")]
        [HasPermission("Roles")]
        public async Task<ActionResult<IEnumerable<PermissionCompactResponse>>> GetPermissionsByIdRole(int id)
        {
            try
            {
                if (!await _roleService.ExistById(id))
                {
                    return NotFound("The Role was not found");
                }

                var permissions = await _permissionsRoleService.GetPermissions(id);
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
        [HasPermission("Roles")]
        public async Task<ActionResult<RoleWithPermissionsResponse>> Create(RoleRequest request)
        {
            try
            {
                if (await _roleService.ExistByName(request.Name))
                {
                    return BadRequest("There is already a Role with the name: " + request.Name);
                }

                var countPermissions = await _permissionService.GetModelPermissionsByIds(request.Permissions);

                if (countPermissions.Count() != request.Permissions.Count())
                {
                    return BadRequest("One or more Permissions not exist");
                }

                var role = await _roleService.Create(request);

                var successSync = await _permissionsRoleService.SyncPermissionByRole(request.Permissions, role.IdRole);
                if (!successSync)
                {
                    throw new Exception("Not Permission sync");
                }

                var roleResponse = await _roleService.GetRoleById(role.IdRole);
                return CreatedAtAction(nameof(GetById), new { Id = roleResponse!.IdRole }, roleResponse);
            }
            catch
            {
                return Problem("An error occurred while trying to create the Role");
            }
        }
        #endregion

        #region PUT /api/roles/{id}
        [HttpPut("{id:int}")]
        [HasPermission("Roles")]
        public async Task<ActionResult> Update(int id, RoleRequest request)
        {
            try
            {
                var role = await _roleService.GetModelActiveById(id);

                if (role == null)
                {
                    return NotFound("The Role was not found");
                }

                if (await _roleService.ExistByName(request.Name) && request.Name.Trim().ToLower() != role.Name.ToLower())
                {
                    return BadRequest("There is already a Role with the name: " + request.Name);
                }

                var countPermissions = await _permissionService.GetModelPermissionsByIds(request.Permissions);

                if (countPermissions.Count() != request.Permissions.Count())
                {
                    return BadRequest("One or more Permissions not exist");
                }

                var successDelete = await _permissionsRoleService.DeleteAllPermissions(id);
                if (!successDelete)
                {
                    throw new Exception("Not delete Permissions");
                }

                var successSync = await _permissionsRoleService.SyncPermissionByRole(request.Permissions, id);
                if (!successSync)
                {
                    throw new Exception("Not Permission sync");
                }

                await _roleService.Update(role, request);

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
        [HasPermission("Roles")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var oRole = await _roleService.GetModelById(id);

                if (oRole == null)
                {
                    return NotFound("The Role was not found");
                }

                await _roleService.ChangeStatus(oRole, false);

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
        [HasPermission("Roles")]
        public async Task<ActionResult> Reactivate(int id)
        {
            try
            {
                var oRole = await _roleService.GetModelById(id);

                if (oRole == null)
                {
                    return NotFound("The Role was not found");
                }

                await _roleService.ChangeStatus(oRole, true);

                return Ok();
            }
            catch
            {
                return Problem("An error occurred while trying to reactive the Role");
            }
        }
        #endregion
    }
}
