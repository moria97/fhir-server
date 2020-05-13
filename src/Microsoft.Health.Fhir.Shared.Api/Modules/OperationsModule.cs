// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Extensions.DependencyInjection;
using Microsoft.Health.Fhir.Core.Features.Operations.Export;

namespace Microsoft.Health.Fhir.Api.Modules
{
    /// <summary>
    /// Registration of operations components.
    /// </summary>
    public class OperationsModule : IStartupModule
    {
        public void Load(IServiceCollection services)
        {
            EnsureArg.IsNotNull(services, nameof(services));

            services.Add<AnonymizeJobTask>()
                .Transient()
                .AsSelf();

            services.Add<ExportJobTask>()
                .Transient()
                .AsSelf();

            services.Add<ExportJobResolver>(sp => type =>
                {
                    if (type == Core.Features.Operations.Export.Models.ExportJobType.Export)
                    {
                        return sp.GetRequiredService<ExportJobTask>();
                    }
                    else
                    {
                        return sp.GetRequiredService<AnonymizeJobTask>();
                    }
                })
                .Transient()
                .AsSelf()
                .AsFactory();

            services.Add<ExportJobWorker>()
                .Singleton()
                .AsSelf();

            services.Add<ResourceToNdjsonBytesSerializer>()
                .Singleton()
                .AsService<IResourceToByteArraySerializer>();
        }
    }
}
