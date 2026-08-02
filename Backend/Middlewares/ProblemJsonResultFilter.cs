using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Middlewares;

public class ProblemJsonResultFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.HttpContext.Response.StatusCode >= 400)
        {
            context.HttpContext.Response.ContentType = "application/problem+json";
        }
    }

    public void OnResultExecuted(ResultExecutedContext context) { }
}