// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Shells.JobShell.Shared;

public struct ShellCommandModel
{
    static string emptyTraceId = Guid.Empty.ToString("N");
    internal const string ENV_PREFIX = "env:";
    internal const string PARAM_PREFIX = "param:";

    internal const string DEFAULT_OTEL_URL = "http://localhost:4317";

    public ShellCommandModel(
        string basePath,
        Guid taskId,
        string jobAssemblyPath,
        string jobEntryClassName,
        string? traceId,
        string? parentSpanId,
        long uTCTicks,
        long offset,
        Guid jobId,
        string? otelUrl = default,
        string? jobStartParam = default,
        string? schedulerEnvironment = default)
    {
        if (string.IsNullOrEmpty(jobAssemblyPath))
            throw new ArgumentNullException(nameof(jobAssemblyPath));
        if (string.IsNullOrEmpty(jobEntryClassName))
            throw new ArgumentNullException(nameof(jobEntryClassName));
        BasePath = basePath;
        TaskId = taskId;
        JobAssemblyPath = jobAssemblyPath;
        JobEntryClassName = jobEntryClassName;
        TraceId = traceId;
        ParentSpanId = parentSpanId;
        UTCTicks = uTCTicks;
        Offset = offset;
        JobId = jobId;
        OtelUrl = string.IsNullOrEmpty(otelUrl) ? DEFAULT_OTEL_URL : otelUrl;
        JobStartParam = jobStartParam;
        SchedulerEnvironment = schedulerEnvironment;
    }

    public string BasePath { get; }

    public Guid TaskId { get; }

    public string JobAssemblyPath { get; }

    public string JobEntryClassName { get; }

    public string? TraceId { get; }

    public string? ParentSpanId { get; }

    public long UTCTicks { get; }

    public long Offset { get; }

    public Guid JobId { get; }

    public string OtelUrl { get; }

    public string? JobStartParam { get; }

    public string? SchedulerEnvironment { get; }

    public override string ToString()
    {
        var values = new List<string>
        {
            BasePath,
            TaskId.ToString(),
            JobAssemblyPath,
            JobEntryClassName,
            string.IsNullOrEmpty(TraceId) ? emptyTraceId : TraceId,
            string.IsNullOrEmpty(ParentSpanId) ? emptyTraceId.Substring(0, 16) : ParentSpanId,
            UTCTicks.ToString(),
            Offset.ToString(),
            JobId.ToString(),
            OtelUrl
        };

        if (!string.IsNullOrEmpty(SchedulerEnvironment))
        {
            values.Add($"{ENV_PREFIX}{Convert.ToBase64String(Encoding.UTF8.GetBytes(SchedulerEnvironment))}");
        }

        if (!string.IsNullOrEmpty(JobStartParam))
        {
            values.Add($"{PARAM_PREFIX}{Convert.ToBase64String(Encoding.UTF8.GetBytes(JobStartParam))}");
        }

        return string.Join(' ', values);
    }
}

public static class ShellCommandModelExtension
{
    public static ShellCommandModel? ToShellCommand(this string[] args, bool hasFirst = false)
    {
        int paramCount = 9, start = 0;
        if (hasFirst)
        {
            paramCount++;
            start++;
        }

        if (args.Length - paramCount < 0)
            throw new ArgumentException($"arg length less than {paramCount}");
        _ = Guid.TryParse(args[start++], out var taskId);
        string path = args[start++], className = args[start++], traceId = args[start++], spanId = args[start++];
        var ticks = Convert.ToInt64(args[start++]);
        var offset = Convert.ToInt64(args[start++]);
        _ = Guid.TryParse(args[start++], out var jobId);
        string otelUrl = args[start++];
        string? schedulerEnv = default;
        string? jobParam = default;

        // Support both old format (single base64 jobParam) and new prefixed extras.
        for (var i = start; i < args.Length; i++)
        {
            var current = args[i];
            if (current.StartsWith(ShellCommandModel.ENV_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                var value = current.Substring(ShellCommandModel.ENV_PREFIX.Length);
                if (!string.IsNullOrEmpty(value))
                {
                    schedulerEnv = Encoding.UTF8.GetString(Convert.FromBase64String(value));
                }
                continue;
            }

            if (current.StartsWith(ShellCommandModel.PARAM_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                var value = current.Substring(ShellCommandModel.PARAM_PREFIX.Length);
                if (!string.IsNullOrEmpty(value))
                {
                    jobParam = Encoding.UTF8.GetString(Convert.FromBase64String(value));
                }
                continue;
            }

            // Legacy format: only one extra argument, it is jobParam in base64.
            jobParam ??= Encoding.UTF8.GetString(Convert.FromBase64String(current));
        }

        return new ShellCommandModel(
            hasFirst ? args[0] : default!,
            taskId,
            path,
            className,
            traceId,
            spanId,
            ticks,
            offset,
            jobId,
            otelUrl,
            jobParam,
            schedulerEnv);
    }
}
