using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace Microsoft.Azure.WebPubSub.AspNetCore
{
    internal class CustomSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema == null)
            {
                schema = new OpenApiSchema();
            }

            schema.Example = new OpenApiObject()
            {

            };
        }
    }
}
