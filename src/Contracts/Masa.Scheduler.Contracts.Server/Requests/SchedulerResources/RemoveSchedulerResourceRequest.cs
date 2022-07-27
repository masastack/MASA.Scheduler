// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Requests.SchedulerResources;

public class RemoveSchedulerResourceRequest : BaseRequest
{
    public Guid ResourceId { get; set; }

    public string JobAppIdentity { get; set; } = string.Empty;
}
