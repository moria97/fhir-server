// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using MediatR;

namespace Microsoft.Health.Fhir.Core.Messages.Export
{
    public class CreateAnonymizeRequest : IRequest<CreateExportResponse>
    {
        public CreateAnonymizeRequest(Uri requestUri, string collectionId)
        {
            EnsureArg.IsNotNull(requestUri, nameof(requestUri));

            RequestUri = requestUri;
            CollectionId = collectionId;
        }

        public Uri RequestUri { get; }

        public string CollectionId { get; }
    }
}
