// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Teams.Queries
{
    public record TeamQuery : Query<List<Team>>
    {
        public override List<Team> Result { get; set; } = new();
    }
}
