// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Projects.Queries;

public record ProjectQuery: Query<List<ProjectDto>>
{
    public Guid? TeamId { get; set; }

    public string Environment { get; set; } = "development";

    public ProjectQuery(Guid? teamId, string enviroment)
    {
        TeamId = teamId;
        Environment = enviroment;
    }

    public override List<ProjectDto> Result { get; set; } = new();
}
