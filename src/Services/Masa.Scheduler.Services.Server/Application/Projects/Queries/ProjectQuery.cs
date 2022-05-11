// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Projects.Queries;

public record ProjectQuery: Query<List<ProjectModel>>
{
    public Guid TeamId { get; set; }

    public ProjectQuery(Guid teamId)
    {
        TeamId = teamId;
    }

    public override List<ProjectModel> Result { get; set; } = new();
}
