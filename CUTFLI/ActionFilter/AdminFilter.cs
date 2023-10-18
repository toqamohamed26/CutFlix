using CUTFLI.Enums;
using CUTFLI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using static CUTFLI.Enums.SystemEnums;

namespace CUTFLI.ActionFilter
{
    public class AdminFilter : IActionFilter
    {
        private readonly CUTFLIDbContext _cUTFLIDbContext;
        public AdminFilter(CUTFLIDbContext cUTFLIDbContext)
        {
            _cUTFLIDbContext = cUTFLIDbContext;
        }
        public void OnActionExecuted(ActionExecutedContext context)
        { }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                int? currentUserId = Convert.ToInt32(context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                var userRole = _cUTFLIDbContext.Users.Where(x => x.Id == currentUserId).FirstOrDefault();
                if (userRole.Permission != SystemEnums.Permission.Admin)
                {
                    context.ModelState.AddModelError("Authorization", "Authorization failed!");
                    context.Result = new BadRequestObjectResult(context.ModelState);
                }
            }
            else
            {
                context.ModelState.AddModelError("Authorization", "Authorization failed!");
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }
    }
}
