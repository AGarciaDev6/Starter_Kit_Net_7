using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Starter_NET_7.DTOs.Request.User;
using Starter_NET_7.DTOs.Response.General;
using Starter_NET_7.DTOs.Response.User;
using Starter_NET_7.Filter;
using Starter_NET_7.Services.Databse;

namespace Starter_NET_7.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly RoleService _roleService;
        private readonly PermissionsRolesService _permissionsRolesService;
        private readonly PermissionsUsersServices _permissionsUsersServices;

        public UserController(
            UserService userService,
            RoleService roleService,
            PermissionsRolesService permissionsRolesService,
            PermissionsUsersServices permissionsUsersServices)
        {
            _userService = userService;
            _roleService = roleService;
            _permissionsRolesService = permissionsRolesService;
            _permissionsUsersServices = permissionsUsersServices;
        }

        #region GET /api/users/get-all/{status:bool}
        [HttpGet("get-all/{status:bool}")]
        [HasPermission("Users")]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllByStatus(bool status)
        {
            try
            {
                var users = await _userService.GetAllByStatus(status);
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
                var users = await _userService.GetAllSelect();
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
        [HasPermission("Users")]
        public async Task<ActionResult<UserWithPermissionsResponse>> GetById(int id)
        {
            try
            {
                var user = await _userService.GetByIdWhitPermission(id);

                if (user == null)
                {
                    return NotFound("The User was not found");
                }

                return Ok(user);
            }
            catch
            {
                return Problem("An error occurred while trying to query the User");
            }
        }
        #endregion

        #region POST /api/users
        [HttpPost]
        [HasPermission("Users")]
        public async Task<ActionResult> Create(UserCreateRequest request)
        {
            try
            {
                if (await _userService.ExistByEmail(request.Email))
                {
                    return BadRequest("There is already a User with the Email: " + request.Email);
                }

                if (!await _roleService.ExistActiveById(request.IdRole))
                {
                    return BadRequest("The Role was not found");
                }

                if (request.Password.Trim() != request.PasswordConfirm.Trim())
                {
                    return BadRequest("Passwords must match");
                }

                var countPermissions = await _permissionsRolesService.GetModelPermissionsByIds(request.Permissions, request.IdRole);

                if (countPermissions.Count() != request.Permissions.Count())
                {
                    return BadRequest("One or more Permissions not exists in the Role");
                }

                var user = await _userService.Create(request);

                var successSync = await _permissionsUsersServices.SyncPermission(request.Permissions, user.IdUser);
                if (!successSync)
                {
                    throw new Exception("Not Permission Sync");
                }

                var userResponse = await _userService.GetById(user.IdUser);
                return CreatedAtAction(nameof(GetById), new { Id = userResponse!.IdUser }, userResponse);
            }
            catch
            {
                return Problem("An error occurred while trying to create the User");
            }
        }
        #endregion

        #region PUT /api/users/{id}
        [HttpPut("{id:int}")]
        [HasPermission("Users")]
        public async Task<ActionResult> Update(int id, UserUpdateRequest request)
        {
            try
            {
                var user = await _userService.GetModelActiveById(id);
                if (user == null)
                {
                    return NotFound("The User was not found");
                }

                if (await _userService.ExistByEmail(request.Email) && request.Email.Trim().ToLower() != user.Email.Trim().ToLower())
                {
                    return BadRequest("There is already a User with the Email: " + request.Email);
                }

                if (! await _roleService.ExistById(request.IdRole))
                {
                    return BadRequest("The Role was not found");
                }

                if (request?.Password?.Trim() != request?.PasswordConfirm?.Trim())
                {
                    return BadRequest("Passwords must match");
                }

                var countPermissions = await _permissionsRolesService.GetModelPermissionsByIds(request!.Permissions, request.IdRole);

                if (countPermissions.Count() != request.Permissions.Count())
                {
                    return BadRequest("One or more Permissions not exists in the Role");
                }

                var successDelete = await _permissionsUsersServices.DeleteAllPermissions(id);
                if (!successDelete)
                {
                    throw new Exception("Not delete Permissions");
                }

                var successSync = await _permissionsUsersServices.SyncPermission(request.Permissions, user.IdUser);
                if (!successSync)
                {
                    throw new Exception("Not Permission Sync");
                }

                await _userService.Update(user, request);

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
        [HasPermission("Users")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var user = await _userService.GetModelById(id);

                if (user == null)
                {
                    return NotFound("The User was not found");
                }

                await _userService.UpdateStatus(user, false);
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
        [HasPermission("Users")]
        public async Task<ActionResult> Reactivate(int id)
        {
            try
            {
                var user = await _userService.GetModelById(id);

                if (user == null)
                {
                    return NotFound("The User was not found");
                }

                await _userService.UpdateStatus(user, true);
                return Ok();
            }
            catch
            {
                return Problem("An error occurred while trying to reactive the User");
            }
        }
        #endregion

    }
}
