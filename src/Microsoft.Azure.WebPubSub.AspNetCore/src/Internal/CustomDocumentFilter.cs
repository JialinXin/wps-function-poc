using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace Microsoft.Azure.WebPubSub.AspNetCore
{
    internal class CustomDocumentFilter : IDocumentFilter
    {
        public const string VersionEndPoint = "/api";
        public void Apply(OpenApiDocument openApiDocument, DocumentFilterContext context)
        {
            // define operation
            var operation = new OpenApiOperation
            {
                Summary = "WebPubSub Upstream"
            };
            // assign tag
            operation.Tags.Add(new OpenApiTag { Name = "WebPubSub Upstream" });

            // create response properties
            var properties = new Dictionary<string, OpenApiSchema>
            {
                { "Version", new OpenApiSchema() { Type = "string" } }
            };

            // create response
            var response = new OpenApiResponse
            {
                Description = "Success"
            };

            // add response type
            response.Content.Add("application/json", new OpenApiMediaType
            {
                Schema = new OpenApiSchema
                {
                    Type = "object",
                    AdditionalPropertiesAllowed = true,
                    Properties = properties,
                }
            });

            // adding response to operation
            operation.Responses.Add("200", response);

            // enable this code if your endpoint requires authorization.
            //operation.Security.Add(new OpenApiSecurityRequirement
            //{
            //    {
            //        new OpenApiSecurityScheme
            //        {
            //            Reference = new OpenApiReference {
            //                Type = ReferenceType.SecurityScheme,
            //                Id = "bearerAuth"
            //            }
            //        }, new List<string>()
            //    }
            //});

            // create path item
            var pathItem = new OpenApiPathItem();
            // add operation to the path
            pathItem.AddOperation(OperationType.Post, operation);
            // finally add the path to document
            openApiDocument?.Paths.Add(VersionEndPoint, pathItem);
        }
    }
}
