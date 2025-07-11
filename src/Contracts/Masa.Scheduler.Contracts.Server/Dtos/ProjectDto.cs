// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Dtos;

public class ProjectDto
{
    public int Id { get; set; }

    public List<Guid> TeamIds { get; set; } = new List<Guid>();

    [Obsolete]
    public Guid TeamId => TeamIds.Count > 0 ? TeamIds[0] : Guid.Empty;

    public string Identity { get; set; } = "";

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public int LabelId { get; set; }

    public string LabelName { get; set; } = "";

    public Guid Modifier { get; set; }

    public DateTime ModificationTime { get; set; }

    public List<ProjectAppDto> ProjectApps { get; set; } = new List<ProjectAppDto>();
}

