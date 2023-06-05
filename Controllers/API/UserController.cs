using Azure.Core;
using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Starter_NET_7.Database.Models;
using Starter_NET_7.DTOs.Request.General;
using Starter_NET_7.DTOs.Request.User;
using Starter_NET_7.DTOs.Response.General;
using Starter_NET_7.DTOs.Response.User;
using Starter_NET_7.Filter;
using Starter_NET_7.Services.Databse;
using System.Runtime.ConstrainedExecution;

namespace Starter_NET_7.Controllers.API
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

        private readonly string _delimiter = "|";

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

                if (!await _roleService.ExistById(request.IdRole))
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

        #region
        [HttpPost("import")]
        //[HasPermission("Import Users")]
        public async Task<ActionResult> ImportUser(IFormFile file)
        {
            try
            {
                List<string> headers = new List<string>() { "Name", "LastName", "Email", "Password", "Role", "Permissions" };
                List<string> headersFile = new List<string>();
                List<RecordErrorResponse> errors = new List<RecordErrorResponse>();
                int recordsOk = 0;
                int recordsError = 0;

                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    stream.Position = 0;

                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        reader.Read();
                        for (int i = 0; i < headers.Count(); i++)
                        {
                            if (reader.GetValue(i) != null)
                            {
                                headersFile.Add(reader.GetValue(i).ToString()!.Trim());
                            }
                        }

                        if (!headers.SequenceEqual(headersFile))
                        {
                            return Problem("Not have headers suport");
                        }

                        int row = 2;
                        while (reader.Read())
                        {

                            var colCounter = 0;
                            for (int i = 0; i < headers.Count(); i++)
                            {
                                if (reader.GetValue(i) == null)
                                {
                                    colCounter++;
                                }
                            }

                            if (headers.Count() == colCounter)
                            {
                                errors.Add(new RecordErrorResponse()
                                {
                                    Message = $"The row {row} is null or empty",
                                    Row = row
                                });

                                recordsError++;
                                row++;
                                continue;
                            }

                            // Name validations
                            string name = reader.GetValue(0) != null ? reader.GetValue(0).ToString().Trim() : "";
                            if (string.IsNullOrEmpty(name))
                            {
                                errors.Add(new RecordErrorResponse()
                                {
                                    Message = string.IsNullOrEmpty(name) ? "Name error, the value is null or empty" : $"Name error, the maxlength is 50 characters, value name: {name}",
                                    Row = row
                                });

                                recordsError++;
                                row++;
                                continue;
                            }

                            // Last name validations
                            string lastName = reader.GetValue(1) != null ? reader.GetValue(1).ToString().Trim() : "";
                            if (string.IsNullOrEmpty(lastName))
                            {
                                errors.Add(new RecordErrorResponse()
                                {
                                    Message = string.IsNullOrEmpty(lastName) ? "Last Name error, the value is null or empty" : $"Last Name error, the maxlength is 50 characters, value last name: {lastName}",
                                    Row = row
                                });

                                recordsError++;
                                row++;
                                continue;
                            }

                            // Email validations
                            string email = reader.GetValue(2) != null ? reader.GetValue(2).ToString().Trim() : "";
                            if (string.IsNullOrEmpty(email))
                            {
                                errors.Add(new RecordErrorResponse()
                                {
                                    Message = string.IsNullOrEmpty(email) ? "Email error, the value is null or empty" : $"Email error, the maxlength is 50 characters, value email: {email}",
                                    Row = row
                                });

                                recordsError++;
                                row++;
                                continue;
                            }

                            // Password validations
                            string password = reader.GetValue(3) != null ? reader.GetValue(3).ToString().Trim() : "";
                            if (string.IsNullOrEmpty(password))
                            {
                                errors.Add(new RecordErrorResponse()
                                {
                                    Message = string.IsNullOrEmpty(password) ? "Password error, the value is null or empty" : $"Password error, the maxlength is 50 characters, value password: {password}",
                                    Row = row
                                });

                                recordsError++;
                                row++;
                                continue;
                            }

                            // Role validations
                            string roleName = reader.GetValue(4) != null ? reader.GetValue(4).ToString().Trim() : "";
                            var oRole = await _roleService.GetModelActiveByName(roleName);

                            if (oRole == null)
                            {
                                errors.Add(new RecordErrorResponse()
                                {
                                    Message = $"Role error, the role: {roleName} was not found",
                                    Row = row
                                });

                                recordsError++;
                                row++;
                                continue;
                            }

                            // Permissions validations
                            string permissions = reader.GetValue(5) != null ? reader.GetValue(5).ToString().Trim() : "";
                            if (string.IsNullOrEmpty(permissions))
                            {
                                errors.Add(new RecordErrorResponse()
                                {
                                    Message = string.IsNullOrEmpty(permissions) ? "Permissions error, the value is null or empty" : $"Permissions error, the maxlength is 50 characters, value permissions: {permissions}",
                                    Row = row
                                });

                                recordsError++;
                                row++;
                                continue;
                            }

                            var permissionsRole = await _permissionsRolesService.GetModelPermissions(oRole.IdRole);
                            List<UnionPermissionsRole> oPermissionsUser = new List<UnionPermissionsRole>();
                            
                            if (permissions.ToLower() == "all")
                            {
                                oPermissionsUser.AddRange(permissionsRole);
                            }
                            else
                            {
                                var permissionsSplit = permissions.Split(_delimiter);

                                foreach (var permission in permissionsSplit)
                                {
                                    var permissionRole = permissionsRole.FirstOrDefault(x => x.Permission.Name.ToLower() == permission.ToLower());
                                    if (permissionRole != null)
                                    {
                                        oPermissionsUser.Add(permissionRole);
                                    } 
                                    else
                                    {
                                        errors.Add(new RecordErrorResponse()
                                        {
                                            Message = $"Permission error, the permission {permission} is not assigned for role {oRole.Name}",
                                            Row = row
                                        });

                                        recordsError++;
                                        continue;
                                    }
                                }
                            }

                            /**
                             * Add 
                             */

                            var permissionsCreate = oPermissionsUser.Select(x => x.PermissionId).ToArray();
                            var user = await _userService.GetModelByEmail(email);

                            if (user == null)
                            {
                                user = await _userService.Create(new UserCreateRequest
                                {
                                    Name = name,
                                    LastName = lastName,
                                    Email = email,
                                    Password = password,
                                    IdRole = oRole.IdRole
                                });

                                var successSync = await _permissionsUsersServices.SyncPermission(permissionsCreate, user.IdUser);
                            }
                            else
                            {
                                var successDelete = await _permissionsUsersServices.DeleteAllPermissions(user.IdUser);
                                var successSync = await _permissionsUsersServices.SyncPermission(permissionsCreate, user.IdUser);

                                await _userService.Update(user, new UserUpdateRequest
                                {
                                    Name = name,
                                    LastName = lastName,
                                    Email = email,
                                    Password = password,
                                    IdRole = oRole.IdRole
                                });
                            }

                            recordsOk++;
                            row++;
                        }
                    }
                }

                var response = new ImportResponse()
                {
                    RecordOk = recordsOk,
                    RecordError = recordsError,
                    Errors = errors
                };

                return Ok(response);
            }
            catch
            {
                return Problem("An error occurred while trying to reactive the Import Users");
            }
        }
        #endregion

    }
}
