using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Comprehension.Controllers
{
    public abstract class ControllerBaseEx : ControllerBase
    {
        protected Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
