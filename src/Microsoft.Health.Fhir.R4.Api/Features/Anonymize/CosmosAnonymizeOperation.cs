// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fhir.Anonymizer.Core;
using Fhir.Anonymizer.Core.AnonymizerConfigurations;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.Health.Fhir.Core.Extensions;
using Microsoft.Health.Fhir.Core.Features.Anonymize;
using Microsoft.Health.Fhir.Core.Features.Persistence;

namespace Microsoft.Health.Fhir.Api.Features.Anonymize
{
    public class CosmosAnonymizeOperation : IAnonymizationOperation
    {
        private Dictionary<string, AnonymizerConfiguration> _configurationDictionary = new Dictionary<string, AnonymizerConfiguration>();
        private IAnonymizeConfigurationStore _fhirOperationDataStore;
        private IResourceWrapperFactory _resourceWrapperFactory;
        private FhirJsonParser _fhirJsonParser;

        public CosmosAnonymizeOperation(
            IAnonymizeConfigurationStore fhirOperationDataStore,
            FhirJsonParser fhirJsonParser,
            IResourceWrapperFactory resourceWrapperFactory)
        {
            _fhirOperationDataStore = fhirOperationDataStore;
            _resourceWrapperFactory = resourceWrapperFactory;
            _fhirJsonParser = fhirJsonParser;
        }

        public async Task<AnonymizerConfiguration> GetConfigurationById(string collectionId)
        {
            if (_configurationDictionary.ContainsKey(collectionId))
            {
                return _configurationDictionary[collectionId];
            }

            return await _fhirOperationDataStore.GetAnonymizerConfigurationByIdAsync(collectionId, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task<ResourceWrapper> Anonymize(ResourceWrapper resource, string collectionId)
        {
            var configuration = await GetConfigurationById(collectionId);
            var engine = new AnonymizerEngine(new AnonymizerConfigurationManager(configuration));
            var anonymizedData = engine.AnonymizeJson(resource.RawResource.Data);
            var anonymizedResource = _fhirJsonParser.Parse<Resource>(anonymizedData).ToResourceElement();
            return _resourceWrapperFactory.Create(anonymizedResource, false);
        }
    }
}
