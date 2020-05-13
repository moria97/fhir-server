﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Core.Features.Persistence
{
    public interface IFhirDataStore
    {
        Task<UpsertOutcome> UpsertAsync(
            ResourceWrapper resource,
            WeakETag weakETag,
            bool allowCreate,
            bool keepHistory,
            CancellationToken cancellationToken,
            string collectionId = null);

        Task<ResourceWrapper> GetAsync(ResourceKey key, CancellationToken cancellationToken, string collectionId = null);

        Task HardDeleteAsync(ResourceKey key, CancellationToken cancellationToken);
    }
}
