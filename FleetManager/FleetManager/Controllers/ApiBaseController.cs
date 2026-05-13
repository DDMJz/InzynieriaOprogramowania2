using FleetManager.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace FleetManager.Controllers
{
    [ApiController]
    public abstract class ApiBaseController : ControllerBase
    {
        protected ActionResult HandleErrorResult(Result result)
        {
            return result.ErrorType switch
            {
                ResultErrorType.NotFound => NotFound(new { message = result.Error }),
                ResultErrorType.Validation => BadRequest(new { message = result.Error }),
                ResultErrorType.Conflict => Conflict(new { message = result.Error }),
                _ => StatusCode(500, new { message = "Wystąpił nieoczekiwany błąd systemu." })
            };
        }
    }
}