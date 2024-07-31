using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VideoCrypt.Image.Dashboard.Models;

namespace VideoCrypt.Image.Dashboard.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;

        var errorResponse = new ErrorResponse
        {
            Success = false
        };
        switch (exception)
        {
            case UnauthorizedAccessException ex:
                logger.LogError(exception.Message);
                // Redirect to the logout page
                context.Response.Redirect("/Account/Logout");
                break;
            default:
                break;
        }

        logger.LogError(exception.Message);
    }
}