// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkImport;

namespace Microsoft.Health.Fhir.CosmosDb
{
    public interface IBulkDocumentClient
    {
        Task InitializeExecutor();

        Task<BulkImportResponse> BulkImport(IEnumerable<string> documentsToImportInBatch, CancellationToken token);
    }
}
