// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Shells.JobShell.Shared;

public struct ShellCommandModel
{
    static string emptyTraceId = Guid.Empty.ToString("N");

    internal const string DEFAULT_OTEL_URL = "http://localhost:4317";

    public ShellCommandModel(string basePath, Guid taskId, string jobAssemblyPath, string jobEntryClassName, string? traceId, string? parentSpanId, long uTCTicks, long offset, Guid jobId, string? otelUrl = default, string? jobStartParam = default)
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

    public override string ToString()
    {
        return string.Join(' ',
            BasePath,
            TaskId,
            JobAssemblyPath,
            JobEntryClassName,
            string.IsNullOrEmpty(TraceId) ? emptyTraceId : TraceId,
            string.IsNullOrEmpty(ParentSpanId) ? emptyTraceId.Substring(0, 16) : ParentSpanId,
            UTCTicks,
            Offset,
            JobId,
            OtelUrl,
            string.IsNullOrEmpty(JobStartParam) ? "" : Convert.ToBase64String(Encoding.UTF8.GetBytes(JobStartParam))
            );
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
        string otelUrl = args[start++],
            jobParam = args.Length - paramCount > 0 ? Encoding.UTF8.GetString(Convert.FromBase64String(args[start])) : default!;
        return new ShellCommandModel(hasFirst ? args[0] : default!, taskId, path, className, traceId, spanId, ticks, offset, jobId, otelUrl, jobParam);
    }
}
