// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------
using Fhir.Anonymizer.Core;
using Fhir.Anonymizer.Core.AnonymizerConfigurations;
using Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Core.Features
{
    public static class ResourceAnonymizeExtension
    {
        public static Resource Anonymize(this Resource resource, AnonymizerConfiguration configuration)
        {
            var engine = new AnonymizerEngine(new AnonymizerConfigurationManager(configuration));
            return engine.AnonymizeResource(resource);
        }
    }
}
