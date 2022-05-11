﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Teams.Queries
{
    public record TeamQuery : Query<List<TeamModel>>
    {
        public override List<TeamModel> Result { get; set; } = new();
    }
}
