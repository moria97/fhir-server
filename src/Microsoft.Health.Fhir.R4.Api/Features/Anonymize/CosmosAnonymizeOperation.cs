// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fhir.Anonymizer.Core;
using Fhir.Anonymizer.Core.AnonymizerConfigurations;
using Fhir.Anonymizer.Core.Extensions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.Health.Fhir.Core.Extensions;
using Microsoft.Health.Fhir.Core.Features.Anonymize;
using Microsoft.Health.Fhir.Core.Features.Persistence;
using Microsoft.Health.Fhir.Core.Features.Search;

namespace Microsoft.Health.Fhir.Api.Features.Anonymize
{
    public class CosmosAnonymizeOperation : IAnonymizationOperation
    {
        private IAnonymizeConfigurationStore _fhirOperationDataStore;
        private IResourceWrapperFactory _resourceWrapperFactory;
        private FhirJsonParser _fhirJsonParser;
        private ISearchIndexer _searchIndexer;

        public CosmosAnonymizeOperation(
            IAnonymizeConfigurationStore fhirOperationDataStore,
            FhirJsonParser fhirJsonParser,
            ISearchIndexer searchIndexer,
            IResourceWrapperFactory resourceWrapperFactory)
        {
            _fhirOperationDataStore = fhirOperationDataStore;
            _resourceWrapperFactory = resourceWrapperFactory;
            _searchIndexer = searchIndexer;
            _fhirJsonParser = fhirJsonParser;
            Hl7.FhirPath.FhirPathCompiler.DefaultSymbolTable.AddExtensionSymbols();
        }

        public async Task<AnonymizerConfiguration> GetConfigurationById(string collectionId)
        {
            return await _fhirOperationDataStore.GetAnonymizerConfigurationByIdAsync(collectionId, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task<ResourceWrapper> Anonymize(ResourceWrapper resource, string collectionId)
        {
            var configuration = await GetConfigurationById(collectionId);
            var engine = new AnonymizerEngine(new AnonymizerConfigurationManager(configuration));
            var anonymizedData = engine.AnonymizeJson(resource.RawResource.Data);
            var resourceElement = _fhirJsonParser.Parse<Resource>(anonymizedData).ToResourceElement();
            resource.SearchIndices = _searchIndexer.Extract(resourceElement);
            resource.RawResource = new RawResource(anonymizedData, resource.RawResource.Format);
            return resource;
        }
    }
}
