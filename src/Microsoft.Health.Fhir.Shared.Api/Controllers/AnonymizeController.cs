// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------
using System.Threading;
using System.Threading.Tasks;
using Fhir.Anonymizer.Core.AnonymizerConfigurations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Health.Fhir.Api.Features.ActionResults;
using Microsoft.Health.Fhir.Api.Features.Anonymize;
using Microsoft.Health.Fhir.Api.Features.Audit;
using Microsoft.Health.Fhir.Api.Features.Headers;
using Microsoft.Health.Fhir.Api.Features.Routing;
using Microsoft.Health.Fhir.Core.Extensions;
using Microsoft.Health.Fhir.Core.Features.Context;
using Microsoft.Health.Fhir.Core.Features.Operations;
using Microsoft.Health.Fhir.Core.Features.Routing;
using Microsoft.Health.Fhir.Core.Messages.Export;
using Microsoft.Health.Fhir.ValueSets;

namespace Microsoft.Health.Fhir.Api.Controllers
{
    [ServiceFilter(typeof(AuditLoggingFilterAttribute))]
    public class AnonymizeController : Controller
    {
        private IMediator _mediator;
        private IFhirRequestContextAccessor _fhirRequestContextAccessor;
        private IUrlResolver _urlResolver;
        private IAnonymizeConfigurationStore _cosmosFhirOperationDataStore;

        public AnonymizeController(
            IMediator mediator,
            IUrlResolver urlResolver,
            IFhirRequestContextAccessor fhirRequestContextAccessor,
            IAnonymizeConfigurationStore cosmosFhirOperationDataStore)
        {
            _mediator = mediator;
            _urlResolver = urlResolver;
            _fhirRequestContextAccessor = fhirRequestContextAccessor;
            _cosmosFhirOperationDataStore = cosmosFhirOperationDataStore;
        }

        [HttpPost]
        [AuditEventType(AuditEventSubType.Export)]
        [Route(KnownRoutes.AnonymizeEndpoint)]
        public async Task<ActionResult> Create(string idParameter, [FromBody] AnonymizerConfiguration configuration)
        {
            await _cosmosFhirOperationDataStore.CreateAnonymizeConfigurationAsync(configuration, idParameter, CancellationToken.None);

            CreateExportResponse response = await _mediator.AnonymizeAsync(_fhirRequestContextAccessor.FhirRequestContext.Uri, idParameter, HttpContext.RequestAborted);
            var exportResult = ExportResult.Accepted();
            exportResult.SetContentLocationHeader(_urlResolver, OperationsConstants.Export, response.JobId);

            return exportResult;
        }
    }
}
