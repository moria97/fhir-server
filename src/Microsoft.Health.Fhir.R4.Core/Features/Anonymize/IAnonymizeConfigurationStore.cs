// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using Fhir.Anonymizer.Core.AnonymizerConfigurations;

namespace Microsoft.Health.Fhir.Core.Features.Anonymize
{
    public interface IAnonymizeConfigurationStore
    {
        Task InitializeCollection(string collectionId);

        Task CreateAnonymizeConfigurationAsync(AnonymizerConfiguration configuration, string collectionId, CancellationToken cancellationToken);

        Task<AnonymizerConfiguration> GetAnonymizerConfigurationByIdAsync(string id, CancellationToken cancellationToken);
    }
}
