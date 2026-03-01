using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum) return;

        var enumDescriptions = Enum.GetValues(context.Type)
            .Cast<Enum>()
            .Select(value =>
            {
                var member = context.Type.GetMember(value.ToString()!)[0];
                var display = member.GetCustomAttribute<DisplayAttribute>();
                var label = display?.Name ?? value.ToString();
                return $"{Convert.ToInt32(value)} = {label}";
            });

        schema.Description += $"<br/><b>Available values:</b><br/>{string.Join("<br/>", enumDescriptions)}";
    }
}