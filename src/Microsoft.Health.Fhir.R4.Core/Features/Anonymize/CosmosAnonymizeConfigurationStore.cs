// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Fhir.Anonymizer.Core.AnonymizerConfigurations;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Health.Abstractions.Exceptions;
using Microsoft.Health.CosmosDb.Configs;
using Microsoft.Health.CosmosDb.Features.Storage;
using Microsoft.Health.Extensions.DependencyInjection;
using Microsoft.Health.Fhir.CosmosDb.Features.Storage.Operations.Export;
using Microsoft.Health.Fhir.CosmosDb.Features.Storage.Versioning;

namespace Microsoft.Health.Fhir.Core.Features.Anonymize
{
    public class CosmosAnonymizeConfigurationStore : IAnonymizeConfigurationStore
    {
        private readonly IScoped<IDocumentClient> _documentClientScope;
        private readonly RetryExceptionPolicyFactory _retryExceptionPolicyFactory;
        private readonly ILogger _logger;
        private readonly ILogger<CollectionInitializer> _logger1;
        private readonly ICollectionInitializer _collectionInitializer;
        private CosmosDataStoreConfiguration _cosmosDataStoreConfiguration;
        private CosmosCollectionConfiguration _cosmosCollectionConfiguration;
        private FhirCollectionUpgradeManager _fhirCollectionUpgradeManager;

        public CosmosAnonymizeConfigurationStore(
            IScoped<IDocumentClient> documentClientScope,
            ICollectionInitializer collectionInitializer,
            CosmosDataStoreConfiguration cosmosDataStoreConfiguration,
            IOptionsMonitor<CosmosCollectionConfiguration> namedCosmosCollectionConfigurationAccessor,
            FhirCollectionUpgradeManager fhirCollectionUpgradeManager,
            RetryExceptionPolicyFactory retryExceptionPolicyFactory,
            ILogger<CosmosAnonymizeConfigurationStore> logger,
            ILogger<CollectionInitializer> logger1)
        {
            EnsureArg.IsNotNull(documentClientScope, nameof(documentClientScope));
            EnsureArg.IsNotNull(cosmosDataStoreConfiguration, nameof(cosmosDataStoreConfiguration));
            EnsureArg.IsNotNull(namedCosmosCollectionConfigurationAccessor, nameof(namedCosmosCollectionConfigurationAccessor));
            EnsureArg.IsNotNull(retryExceptionPolicyFactory, nameof(retryExceptionPolicyFactory));
            EnsureArg.IsNotNull(logger, nameof(logger));

            _documentClientScope = documentClientScope;
            _retryExceptionPolicyFactory = retryExceptionPolicyFactory;
            _collectionInitializer = collectionInitializer;
            _logger = logger;
            _logger1 = logger1;

            _cosmosCollectionConfiguration = namedCosmosCollectionConfigurationAccessor.Get("fhirCosmosDb");
            _cosmosDataStoreConfiguration = cosmosDataStoreConfiguration;
            _fhirCollectionUpgradeManager = fhirCollectionUpgradeManager;

            DatabaseId = cosmosDataStoreConfiguration.DatabaseId;
            CollectionId = _cosmosCollectionConfiguration.CollectionId;
            CollectionUri = cosmosDataStoreConfiguration.GetRelativeCollectionUri(_cosmosCollectionConfiguration.CollectionId);
        }

        private string DatabaseId { get; }

        private string CollectionId { get; }

        private Uri CollectionUri { get; }

        public async Task InitializeCollection(string collectionId)
        {
            EnsureArg.IsNotNull(collectionId, nameof(collectionId));

            var collectionInitializer = new CollectionInitializer(
                collectionId,
                _cosmosDataStoreConfiguration,
                _cosmosCollectionConfiguration.InitialCollectionThroughput,
                _fhirCollectionUpgradeManager,
                _logger1);

            await collectionInitializer.InitializeCollection(_documentClientScope.Value);
        }

        public async Task CreateAnonymizeConfigurationAsync(AnonymizerConfiguration configuration, string collectionId, CancellationToken cancellationToken)
        {
            EnsureArg.IsNotNull(configuration, nameof(configuration));

            var configurationWrapper = new CosmosAnonymizationConfigurationWrapper(configuration, collectionId);

            try
            {
                ResourceResponse<Document> result = await _documentClientScope.Value.CreateDocumentAsync(
                    CollectionUri,
                    configurationWrapper,
                    new RequestOptions() { PartitionKey = new PartitionKey(CosmosDbExportConstants.AnonymizeJobPartitionKey) },
                    disableAutomaticIdGeneration: true,
                    cancellationToken: cancellationToken);

                return;
            }
            catch (DocumentClientException dce)
            {
                if (dce.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    throw new RequestRateExceededException(dce.RetryAfter);
                }
                else if (dce.StatusCode == HttpStatusCode.Conflict)
                {
                    return;
                }

                _logger.LogError(dce, "Failed to create a configuration file.");
                throw;
            }
        }

        public async Task<AnonymizerConfiguration> GetAnonymizerConfigurationByIdAsync(string id, CancellationToken cancellationToken)
        {
            EnsureArg.IsNotNullOrWhiteSpace(id, nameof(id));

            try
            {
                DocumentResponse<CosmosAnonymizationConfigurationWrapper> configurationWrapper = await _documentClientScope.Value.ReadDocumentAsync<CosmosAnonymizationConfigurationWrapper>(
                    UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id),
                    new RequestOptions { PartitionKey = new PartitionKey(CosmosDbExportConstants.AnonymizeJobPartitionKey) },
                    cancellationToken);

                return configurationWrapper.Document.Configuration;
            }
            catch (DocumentClientException dce)
            {
                if (dce.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    throw new RequestRateExceededException(dce.RetryAfter);
                }
                else if (dce.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }
        }
    }
}
