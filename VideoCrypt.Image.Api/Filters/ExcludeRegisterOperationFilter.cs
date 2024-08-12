using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace VideoCrypt.Image.Api.Filters;

public class ExcludeRegisterOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var actionApiDescription = context.ApiDescription.ActionDescriptor.DisplayName;

        // Exclude the Register endpoint from Swagger
        if (actionApiDescription != null && actionApiDescription.Contains("/register", StringComparison.InvariantCultureIgnoreCase))
        {
            operation.Responses.Clear();
            operation.Summary = "This endpoint is not available in Swagger";
            operation.Description = "This endpoint is not available in Swagger";
            operation.OperationId = null;
            operation.Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Excluded" } };
        }
    }
}