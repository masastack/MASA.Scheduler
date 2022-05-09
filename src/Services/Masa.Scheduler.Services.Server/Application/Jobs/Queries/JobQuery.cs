// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Jobs.Queries
{
    public record JobQuery : DomainQuery<List<Job>>
    {
        public override List<Job> Result { get; set; } = new();
    }
}