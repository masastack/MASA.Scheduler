// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Projects.Queries;

public record ProjectDetailsQuery : Query<ProjectDto>
{
    public string ProjectIdentity { get; set; } = string.Empty;

    public override ProjectDto Result { get; set; } = new();
}
