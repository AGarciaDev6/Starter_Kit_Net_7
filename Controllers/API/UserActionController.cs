using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Starter_NET_7.Controllers.API
{
    [Authorize]
    [ApiController]
    [Route("api/users/action")]
    public class UserActionController : ControllerBase
    {
        public UserActionController() { }

        
    }
}
