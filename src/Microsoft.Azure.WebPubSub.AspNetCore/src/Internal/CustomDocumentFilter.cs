using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

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
                Summary = "WebPubSub Upstream",
                Parameters = BuildParameters(),
                RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<String, OpenApiMediaType>
                    {
                        {
                            Constants.ContentTypes.PlainTextContentType, new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Type = "string",
                                },
                                Example = new OpenApiString("hello"),
                            }
                        }
                    }
                }
            };
            // assign tag
            operation.Tags.Add(new OpenApiTag { Name = "WebPubSub Upstream" });

            // create response properties
            //var properties = new Dictionary<string, OpenApiSchema>
            //{
            //    { "Version", new OpenApiSchema() { Type = "string" } }
            //};

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
                    //Properties = properties,
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

        private static List<OpenApiParameter> BuildParameters()
        {
            var connectionId = Guid.NewGuid().ToString();
            var headerDict = new Dictionary<string, string>()
            {
                { Constants.Headers.CloudEvents.WebPubSubVersion, "1.0" },
                { Constants.Headers.CloudEvents.SpecVersion, "1.0" },
                { Constants.Headers.CloudEvents.Time, GetTimeString() },
                { Constants.Headers.CloudEvents.Hub, "samplehub" },
                { Constants.Headers.CloudEvents.ConnectionId, connectionId },
                { Constants.Headers.CloudEvents.Signature, ComputeHash(connectionId) },
                { Constants.Headers.CloudEvents.Source, $"/hubs/samplehub/client/{connectionId}" },
                { Constants.Headers.WebHookRequestOrigin, "localhost" },
                //{ Constants.Headers.CloudEvents.Type, Constants.Headers.CloudEvents.TypeSystemPrefix + Constants.Events.ConnectedEvent },
                { Constants.Headers.CloudEvents.Type, Constants.Headers.CloudEvents.TypeUserPrefix + Constants.Events.MessageEvent },
                { Constants.Headers.CloudEvents.EventName, Constants.Events.MessageEvent },
            };
            var parameters = new List<OpenApiParameter>();
            foreach (var item in headerDict)
            {
                parameters.Add(new OpenApiParameter
                {
                    Name = item.Key,
                    In = ParameterLocation.Header,
                    //Required = true,
                    Schema = new OpenApiSchema
                    {
                        Type = "String",
                    },
                    AllowReserved = true,
                    Example = new OpenApiString(item.Value),
                });
            }

            return parameters;
        }

        private static string ComputeHash(string connectionId)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGH"));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(connectionId));
            return "sha256=" + BitConverter.ToString(hashBytes).Replace("-", "");
        }

        private static string GetTimeString()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
    }
}
