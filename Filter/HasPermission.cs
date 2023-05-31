using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Starter_NET_7.Interfaces;
using Starter_NET_7.Database;

namespace Starter_NET_7.Filter
{
    public class HasPermission : ActionFilterAttribute
    {
        private readonly string _permission;
        private readonly string _messageResponse = "Not have permissions";

        public HasPermission(string permission)
        {
            this._permission = permission;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var token = context.HttpContext.RequestServices.GetService<IToken>();
            var dbContext = context.HttpContext.RequestServices.GetService<AppDbContext>();

            var idUser = token!.GetIdUserOfToken();

            if (idUser == -1)
            {
                context.HttpContext.Response.StatusCode = 403;
                context.Result = new ForbidResult(_messageResponse);
            }

            var permissions = dbContext!.UnionPermissionsUsers.Include(x => x.Permission).Where(x => x.UserId == idUser && x.Status == true)
                .Select(x => new
                {
                    Name = x.Permission.Name,
                }).ToArray();

            if (!permissions.Any(x => x.Name.ToLower() == _permission.ToLower()))
            {
                context.HttpContext.Response.StatusCode = 403;
                context.Result = new ForbidResult(_messageResponse);
            }

            base.OnActionExecuting(context);
        }
    }
}
