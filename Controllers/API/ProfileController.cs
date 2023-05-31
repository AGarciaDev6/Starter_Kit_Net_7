using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Starter_NET_7.DTOs.Request.Profile;
using Starter_NET_7.DTOs.Response.User;
using Starter_NET_7.Interfaces;
using Starter_NET_7.Services.Databse;

namespace Starter_NET_7.Controllers.API
{
    [Authorize]
    [ApiController]
    [Route("api/users")]
    public class ProfileController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IToken _token;

        public ProfileController(
            UserService userService,
            IToken token)
        {
            _userService = userService;
            _token = token;
        }

        #region
        [HttpGet]
        public async Task<ActionResult<UserResponse>> GetProfile()
        {
            try
            {
                var idUser = _token.GetIdUserOfToken();
                var user = await _userService.GetById(idUser);

                if (user == null)
                {
                    return NotFound("The Profile was not found");
                }

                return Ok(user);
            }
            catch
            {
                return Problem("An error occurred while trying to query the Profile");
            }
        }
        #endregion

        #region
        [HttpPut]
        public async Task<ActionResult> Update(ProfileRequest request)
        {
            try
            {
                var idUser = _token.GetIdUserOfToken();
                var user = await _userService.GetModelActiveById(idUser);

                if (user == null)
                {
                    return NotFound("The Profile was not found");
                }

                await _userService.UpdateProfile(user, request);

                return NoContent();
            }
            catch
            {
                return Problem("An error occurred while trying to update the Profile");
            }
        }
        #endregion
    }
}
