using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.WebPubSub.AspNetCore
{
    internal class CustomOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "ce-awpsversion",
                In = ParameterLocation.Header,
                Required = true,
                //Schema = new OpenApiSchema
                //{
                //    Type = "String",
                //},
                Description = "1.0",
                //AllowReserved = true,
            });
        }
    }
}
