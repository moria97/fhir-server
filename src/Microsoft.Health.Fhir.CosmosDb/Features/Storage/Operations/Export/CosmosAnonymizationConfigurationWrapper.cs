// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Fhir.Anonymizer.Core.AnonymizerConfigurations;
using Microsoft.Health.CosmosDb.Features.Storage;
using Microsoft.Health.Fhir.Core.Features.Operations.Export.Models;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.CosmosDb.Features.Storage.Operations.Export
{
    /// <summary>
    /// A wrapper around the <see cref="ExportJobRecord"/> class that contains metadata specific to CosmosDb.
    /// </summary>
    public class CosmosAnonymizationConfigurationWrapper
    {
        public CosmosAnonymizationConfigurationWrapper(AnonymizerConfiguration anonymizerConfigurations, string collectionId)
        {
            EnsureArg.IsNotNull(anonymizerConfigurations, nameof(anonymizerConfigurations));

            Configuration = anonymizerConfigurations;
            Id = collectionId;
        }

        [JsonConstructor]
        protected CosmosAnonymizationConfigurationWrapper()
        {
        }

        [JsonProperty("configuration")]
        public AnonymizerConfiguration Configuration { get; private set; }

        [JsonProperty(KnownDocumentProperties.PartitionKey)]
        public string PartitionKey { get; } = CosmosDbExportConstants.AnonymizeJobPartitionKey;

        [JsonProperty(KnownDocumentProperties.Id)]
        public string Id { get; set; }

        [JsonProperty(KnownDocumentProperties.ETag)]
        public string ETag { get; protected set; }

        [JsonProperty(KnownDocumentProperties.IsSystem)]
        public bool IsSystem { get; } = true;
    }
}
