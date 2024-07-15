namespace VideoCrypt.Image.Server.Middlewares;

public class UserIdValidationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;

        if (!path.Contains("/cache/", StringComparison.OrdinalIgnoreCase))
        {
            if (!context.Request.Headers.ContainsKey("X-UserId"))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("X-UserId header is required.");
                return;
            }
        }

        await _next(context);
    }
}