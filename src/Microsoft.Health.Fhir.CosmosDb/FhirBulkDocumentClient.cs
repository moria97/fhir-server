// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkImport;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;
using Microsoft.Health.CosmosDb.Configs;
using Microsoft.Health.Extensions.DependencyInjection;
using Microsoft.Health.Fhir.CosmosDb.Features.Storage;

namespace Microsoft.Health.Fhir.CosmosDb
{
    public class FhirBulkDocumentClient : IBulkDocumentClient
    {
        private BulkExecutor _bulkExecutor;
        private readonly IScoped<IDocumentClient> _documentClient;
        private readonly CosmosDataStoreConfiguration _cosmosDataStoreConfiguration;
        private readonly IOptionsSnapshot<CosmosCollectionConfiguration> _namedCosmosCollectionConfigurationAccessor;

        public FhirBulkDocumentClient(
            IScoped<IDocumentClient> documentClient,
            CosmosDataStoreConfiguration cosmosDataStoreConfiguration,
            IOptionsSnapshot<CosmosCollectionConfiguration> namedCosmosCollectionConfigurationAccessor)
        {
            _documentClient = documentClient;
            _cosmosDataStoreConfiguration = cosmosDataStoreConfiguration;
            _namedCosmosCollectionConfigurationAccessor = namedCosmosCollectionConfigurationAccessor;

            CosmosCollectionConfiguration collectionConfiguration = _namedCosmosCollectionConfigurationAccessor.Get(Constants.CollectionConfigurationName);
            var collectionUri = _cosmosDataStoreConfiguration.GetRelativeCollectionUri(collectionConfiguration.SinkCollectionId);

            var client = (_documentClient.Value as FhirDocumentClient)._inner as DocumentClient;
            client.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 30;
            client.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 9;

            var dataCollection = client.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(_cosmosDataStoreConfiguration.DatabaseId))
                .Where(c => c.Id == collectionConfiguration.SinkCollectionId).AsEnumerable().FirstOrDefault();
            _bulkExecutor = new BulkExecutor(client, dataCollection);
        }

        public async Task InitializeExecutor()
        {
            await _bulkExecutor.InitializeAsync();
        }

        public async Task<BulkImportResponse> BulkImport(IEnumerable<string> documentsToImportInBatch, CancellationToken token)
        {
            BulkImportResponse bulkImportResponse = await _bulkExecutor.BulkImportAsync(
                documents: documentsToImportInBatch,
                enableUpsert: true,
                disableAutomaticIdGeneration: true,
                maxConcurrencyPerPartitionKeyRange: null,
                maxInMemorySortingBatchSize: null,
                cancellationToken: token);
            return bulkImportResponse;
        }
    }
}
