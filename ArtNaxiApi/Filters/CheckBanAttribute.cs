using ArtNaxiApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace ArtNaxiApi.Filters
{
    public class CheckBanAttribute : ActionFilterAttribute
    {
        private readonly IUserRepository _userRepository;

        public CheckBanAttribute(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new ForbidResult();
                return;
            }

            var user = await _userRepository.GetUserByIdAsync(Guid.Parse(userId));

            if (user.IsBanned)
            {
                context.Result = new JsonResult(new { message = "Your account has been banned." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            await next();
        }
    }
}
