using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApp.Helpers;
public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            foreach (var name in Enum.GetNames(context.Type))
            {
                schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(name));
            }
        }
    }
}