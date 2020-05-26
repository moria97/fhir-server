// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using Fhir.Anonymizer.Core;
using Fhir.Anonymizer.Core.AnonymizerConfigurations;
using Fhir.Anonymizer.Core.Extensions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.Health.Fhir.Core.Extensions;
using Microsoft.Health.Fhir.Core.Features.Persistence;
using Microsoft.Health.Fhir.Core.Features.Search;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.Health.Fhir.Core.Features.Anonymize
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

        public async Task InitializeDataCollection(string collectionId)
        {
            await _fhirOperationDataStore.InitializeCollection(collectionId);
        }

        public async Task<AnonymizerEngine> GetEngineByCollectionId(string collectionId)
        {
            var configuration = await GetConfigurationById(collectionId);
            return new AnonymizerEngine(new AnonymizerConfigurationManager(configuration));
        }

        public async Task<AnonymizerConfiguration> GetConfigurationById(string collectionId)
        {
            return await _fhirOperationDataStore.GetAnonymizerConfigurationByIdAsync(collectionId, CancellationToken.None).ConfigureAwait(false);
        }

        public ResourceWrapper Anonymize(ResourceWrapper resourceWrapper, AnonymizerEngine engine)
        {
            var resource = _fhirJsonParser.Parse<Resource>(resourceWrapper.RawResource.Data);

            var anonymizedResource = engine.AnonymizeResource(resource);
            resourceWrapper.SearchIndices = _searchIndexer.Extract(anonymizedResource.ToResourceElement());
            resourceWrapper.RawResource = new RawResource(anonymizedResource.ToJson(), resourceWrapper.RawResource.Format);
            return resourceWrapper;
        }

        public ResourceWrapper CopyWithoutAnonymize(ResourceWrapper resourceWrapper)
        {
            var resource = _fhirJsonParser.Parse<Resource>(resourceWrapper.RawResource.Data);
            resourceWrapper.SearchIndices = _searchIndexer.Extract(resource.ToResourceElement());
            resourceWrapper.RawResource = new RawResource(resource.ToJson(), resourceWrapper.RawResource.Format);
            return resourceWrapper;
        }
    }
}
