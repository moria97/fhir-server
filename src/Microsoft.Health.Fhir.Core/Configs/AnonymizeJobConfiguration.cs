// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Fhir.Core.Configs
{
    public class AnonymizeJobConfiguration
    {
        public string CollectionId { get; set; }

        public ushort MaximumNumberOfConcurrentJobsAllowed { get; set; } = 1;

        public TimeSpan JobHeartbeatTimeoutThreshold { get; set; } = TimeSpan.FromMinutes(10);

        public TimeSpan JobPollingFrequency { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Controls how many resources will be returned for each search query while exporting the data.
        /// </summary>
        public uint MaximumNumberOfResourcesPerQuery { get; set; } = 1000;

        /// <summary>
        /// Number of pages to be iterated before committing the export progress.
        /// </summary>
        public uint NumberOfPagesPerCommit { get; set; } = 10;
    }
}
