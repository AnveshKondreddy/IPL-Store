using IPLStore.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace IPLStore.API.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult ToActionResult<T>(this Result<T> result)
        {
            if (result.IsSuccess)
                return new OkObjectResult(result.Value);

            var body = new { message = result.Error };

            return result.ErrorKind switch
            {
                ErrorKind.NotFound => new NotFoundObjectResult(body),
                ErrorKind.Conflict => new ConflictObjectResult(body),
                _ => new BadRequestObjectResult(body)
            };
        }
    }
}
