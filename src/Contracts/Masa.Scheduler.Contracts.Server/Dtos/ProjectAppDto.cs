// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Dtos;

public class ProjectAppDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Identity { get; set; } = string.Empty;

    public int ProjectId { get; set; }

    public ProjectAppTypes Type { get; set; }
}

