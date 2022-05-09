// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Resources
{
    public class SchedulerResource : AuditAggregateRoot<Guid, Guid>, ISoftDelete
    {
        public bool IsDeleted { get; private set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public string DownloadUrl { get; set; } = string.Empty;

        public SchedulerResourceTypes ResourceType { get; set; }
    }
}
