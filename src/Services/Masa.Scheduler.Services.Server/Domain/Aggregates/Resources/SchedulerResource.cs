// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Resources;

public class SchedulerResource : FullAggregateRoot<Guid, Guid>
{
    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string FilePath { get; private set; } = string.Empty;

    public string Version { get; private set; } = string.Empty;

    public int JobAppId { get; private set; }

    public DateTimeOffset UploadTime { get; private set; } = DateTimeOffset.MinValue;

    public SchedulerResource(int jobAppId, string name, string description, string filePath, string version, DateTimeOffset uploadTime)
    {
        JobAppId = jobAppId;
        Name = name;
        Description = description;
        FilePath = filePath;
        Version = version;
        UploadTime = uploadTime;
    }

    public void UpdateResouce(string name, string description, string version, string filePath)
    {
        Name = name;
        Description = description;
        Version = version;
        FilePath = filePath;
    }
}
