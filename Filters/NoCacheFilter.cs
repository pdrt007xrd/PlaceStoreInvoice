using Microsoft.AspNetCore.Mvc.Filters;

namespace Ventas.Filters;

public class NoCacheFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var headers = context.HttpContext.Response.Headers;
        headers.CacheControl = "no-cache, no-store, must-revalidate";
        headers.Pragma = "no-cache";
        headers.Expires = "0";
    }
}
