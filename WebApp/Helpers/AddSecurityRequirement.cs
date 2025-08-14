using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApp.Helpers;

public class AddSecurityRequirement : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorizeAttribute = context.MethodInfo.DeclaringType!.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
                                    context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

        var hasAllowAnonymousAttribute = context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();

        if (hasAuthorizeAttribute && !hasAllowAnonymousAttribute)
        {
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new() {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                }
            };
        }
    }
}
