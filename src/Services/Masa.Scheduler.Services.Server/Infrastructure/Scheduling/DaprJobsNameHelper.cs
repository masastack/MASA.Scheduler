// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Scheduling;

public static class DaprJobsNameHelper
{
    private const string CronPrefix = "cron";
    private const string RetryPrefix = "retry";

    public static string BuildCronName(string environment, Guid jobId)
    {
        return $"{CronPrefix}.{environment}.{jobId:N}";
    }

    public static string BuildRetryName(string environment, Guid jobId, Guid taskId)
    {
        return $"{RetryPrefix}.{environment}.{jobId:N}.{taskId:N}";
    }

    public static bool TryParse(string name, out DaprJobNameInfo info)
    {
        info = default;

        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var parts = name.Split('.', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 3)
        {
            return false;
        }

        var prefix = parts[0];
        var tail = parts.Skip(1).ToArray();

        if (string.Equals(prefix, CronPrefix, StringComparison.OrdinalIgnoreCase))
        {
            if (tail.Length < 2)
            {
                return false;
            }

            var jobIdText = tail[^1];

            if (!TryParseGuid(jobIdText, out var jobId))
            {
                return false;
            }

            var environment = string.Join('.', tail.Take(tail.Length - 1));
            info = DaprJobNameInfo.Cron(environment, jobId);
            return true;
        }

        if (string.Equals(prefix, RetryPrefix, StringComparison.OrdinalIgnoreCase))
        {
            if (tail.Length < 3)
            {
                return false;
            }

            var taskIdText = tail[^1];
            var jobIdText = tail[^2];

            if (!TryParseGuid(jobIdText, out var jobId) || !TryParseGuid(taskIdText, out var taskId))
            {
                return false;
            }

            var environment = string.Join('.', tail.Take(tail.Length - 2));
            info = DaprJobNameInfo.Retry(environment, jobId, taskId);
            return true;
        }

        return false;
    }

    private static bool TryParseGuid(string value, out Guid guid)
    {
        if (Guid.TryParseExact(value, "N", out guid))
        {
            return true;
        }

        return Guid.TryParse(value, out guid);
    }
}

public readonly record struct DaprJobNameInfo
{
    public string Environment { get; init; }
    public Guid JobId { get; init; }
    public Guid TaskId { get; init; }
    public DaprJobNameType Type { get; init; }

    public static DaprJobNameInfo Cron(string environment, Guid jobId)
        => new() { Environment = environment, JobId = jobId, Type = DaprJobNameType.Cron };

    public static DaprJobNameInfo Retry(string environment, Guid jobId, Guid taskId)
        => new() { Environment = environment, JobId = jobId, TaskId = taskId, Type = DaprJobNameType.Retry };
}

public enum DaprJobNameType
{
    Cron = 1,
    Retry
}
