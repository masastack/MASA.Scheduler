// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Dtos;

public class SchedulerResourceDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool EnableDescription { get; set; } = false;

    public string FilePath { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string JobAppIdentity { get; set; } = string.Empty;

    public DateTimeOffset UploadTime { get; set; }

    public Guid Creator { get; set; }

  
}

