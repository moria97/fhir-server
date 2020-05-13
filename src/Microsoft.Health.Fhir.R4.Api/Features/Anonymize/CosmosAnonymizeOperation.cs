// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fhir.Anonymizer.Core.AnonymizerConfigurations;
using Microsoft.Health.Fhir.Core.Features.Anonymize;
using Microsoft.Health.Fhir.Core.Features.Persistence;

namespace Microsoft.Health.Fhir.Api.Features.Anonymize
{
    public class CosmosAnonymizeOperation : IAnonymizationOperation
    {
        private Dictionary<string, AnonymizerConfiguration> _configurationDictionary = new Dictionary<string, AnonymizerConfiguration>();
        private IAnonymizeConfigurationStore _fhirOperationDataStore;

        public CosmosAnonymizeOperation(IAnonymizeConfigurationStore fhirOperationDataStore)
        {
            _fhirOperationDataStore = fhirOperationDataStore;
        }

        public async Task<AnonymizerConfiguration> GetConfigurationById(string collectionId)
        {
            if (_configurationDictionary.ContainsKey(collectionId))
            {
                return _configurationDictionary[collectionId];
            }

            return await _fhirOperationDataStore.GetAnonymizerConfigurationByIdAsync(collectionId, CancellationToken.None).ConfigureAwait(false);
        }

        public ResourceWrapper Anonymize(ResourceWrapper resource, string collectionId)
        {
            throw new System.NotImplementedException();
        }
    }
}
