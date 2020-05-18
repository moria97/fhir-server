// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net.Mime;
using EnsureThat;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Health.Fhir.Api.Extensions;
using Microsoft.Health.Fhir.Api.Features.Routing;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.Builder
{
    public static class FhirServerApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds FHIR server functionality to the pipeline.
        /// </summary>
        /// <param name="app">The application builder instance.</param>
        /// <returns>THe application builder instance.</returns>
        public static IApplicationBuilder UseFhirServer(this IApplicationBuilder app)
        {
            EnsureArg.IsNotNull(app, nameof(app));

            app.UseHealthChecks(new PathString(KnownRoutes.HealthCheck), new HealthCheckOptions
            {
                ResponseWriter = async (httpContext, healthReport) =>
                {
                    var response = JsonConvert.SerializeObject(
                        new
                        {
                            overallStatus = healthReport.Status.ToString(),
                            details = healthReport.Entries.Select(entry => new
                            {
                                name = entry.Key,
                                status = Enum.GetName(typeof(HealthStatus), entry.Value.Status),
                                description = entry.Value.Description,
                            }),
                        });

                    httpContext.Response.ContentType = MediaTypeNames.Application.Json;
                    await httpContext.Response.WriteAsync(response);
                },
            });

            app.Use(async (context, next) =>
            {
                var url = context.Request.Path.Value;

                if (context.Request.IsAnonymizeRequest() && !context.Request.IsAnonymizeCreateRequest())
                {
                    // rewrite to search controller
                    var components = context.Request.Path.Value.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    if (components.Length >= 2)
                    {
                        context.Request.Path = "/" + string.Join('/', components.Skip(2));
                    }
                }

                await next();
            });

            app.UseStaticFiles();
            app.UseMvc();

            return app;
        }
    }
}
