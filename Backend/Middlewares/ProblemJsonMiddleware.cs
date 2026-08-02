namespace Api.Middlewares;

public class ProblemJsonMiddleware
{
    private readonly RequestDelegate _next;

    public ProblemJsonMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (context.Response.StatusCode >= 400 && !context.Response.HasStarted)
        {
            context.Response.ContentType = "application/problem+json";
        }
    }
}