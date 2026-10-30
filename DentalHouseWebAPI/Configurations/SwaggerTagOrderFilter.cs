using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DentalHouseWebAPI.Configurations
{
    public class SwaggerTagOrderFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var tags = swaggerDoc.Paths.Values
                .SelectMany(p => p.Operations.Values)
                .SelectMany(o => o.Tags.Select(t => t.Name))
                .Distinct()
                .OrderBy(t => t == "Auth" ? 0 : 1)
                .ThenBy(t => t)
                .Select(t => new OpenApiTag { Name = t })
                .ToList();

            swaggerDoc.Tags = tags;
        }
    }
}
