// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Fhir.Anonymizer.Core;
using Microsoft.Health.Fhir.Core.Features.Persistence;

namespace Microsoft.Health.Fhir.Core.Features.Anonymize
{
    public interface IAnonymizationOperation
    {
        Task InitializeDataCollection(string collectionId);

        Task<AnonymizerEngine> GetEngineByCollectionId(string collectionId);

        ResourceWrapper Anonymize(ResourceWrapper resource, AnonymizerEngine engine);

        ResourceWrapper CopyWithoutAnonymize(ResourceWrapper resourceWrapper);
    }
}
