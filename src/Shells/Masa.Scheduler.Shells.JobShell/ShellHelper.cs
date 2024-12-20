// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Shells.JobShell;

internal sealed class ShellHelper
{
    private ShellHelper() { }

    private static readonly ActivitySource activitySource = new("Scheduler.Shell");

    private static TracerProvider? tracerProvider;

    public static void UseOpenTelemtry(WebApplicationBuilder builder, ShellCommandModel cmd)
    {
        var options = new MasaObservableOptions
        {
            ServiceNameSpace = builder.Environment.EnvironmentName,
            ServiceVersion = "1.0.0",
            ServiceName = "masa-scheduler-job-shell",
            Layer = "masastack",
            ServiceInstanceId = builder.Configuration.GetValue<string>("HOSTNAME")!
        };

        var resources = ResourceBuilder.CreateDefault().AddMasaService(options);

        tracerProvider = Sdk.CreateTracerProviderBuilder().ConfigureResource(resource => resource.AddMasaService(options))
            .AddSource(activitySource.Name)
            .AddOtlpExporter(ot =>
            {
                ot.Endpoint = new Uri(cmd.OtelUrl);
                ot.BatchExportProcessorOptions.ScheduledDelayMilliseconds = 1;
                ot.BatchExportProcessorOptions.MaxExportBatchSize = 1;
            })
            .Build();

        builder.Services.AddLogging(log =>
        {
            log.AddOpenTelemetry(op =>
            {
                op.SetResourceBuilder(resources);
                op.AddOtlpExporter((ot, process) =>
                {
                    ot.Endpoint = new Uri(cmd.OtelUrl);
                    ot.BatchExportProcessorOptions.ScheduledDelayMilliseconds = 1;
                    ot.BatchExportProcessorOptions.MaxExportBatchSize = 1;

                    process.ExportProcessorType = ExportProcessorType.Simple;
                    process.BatchExportProcessorOptions.ScheduledDelayMilliseconds = 1;
                    process.BatchExportProcessorOptions.MaxExportBatchSize = 1;
                });
            });
        });
    }

    public static async Task Execute(IServiceProvider service, ShellCommandModel cmd)
    {
        var result = new RunResult() { TaskId = cmd.TaskId };
        Assembly assembly;
        try
        {
            assembly = Assembly.LoadFrom(cmd.JobAssemblyPath);
        }
        catch (Exception ex)
        {
            result.Message = "Load Assembly Error, Exception Message: " + ex.Message;
            Console.WriteLine(JsonSerializer.Serialize(result));
            return;
        }

        var assemblyType = assembly.GetType(cmd.JobEntryClassName);

        if (assemblyType == null)
        {
            result.Message = $"Assembly type not found, ClassName: {cmd.JobEntryClassName}";
            Console.WriteLine(JsonSerializer.Serialize(result));
            return;
        }

        if (assemblyType.GetInterface(typeof(ISchedulerJob).Name) == null)
        {
            result.Message = $"Class: {assemblyType.Name} not implement ISchedulerJob";
            Console.WriteLine(JsonSerializer.Serialize(result));
            return;
        }

        string[]? parameterArr = cmd.JobStartParam?.Split(';');

        var excuteTime = DateTimeOffset.UtcNow;

        var parseExcuteTime = new DateTimeOffset(cmd.UTCTicks, TimeSpan.FromTicks(cmd.Offset));

        if (parseExcuteTime != DateTimeOffset.MinValue)
        {
            excuteTime = parseExcuteTime;
        }

        var jobId = cmd.JobId;

        var instance = Activator.CreateInstance(assemblyType) as ISchedulerJob;

        if (instance == null)
        {
            result.Message = $"Cannot create ISchedulerJob instance";
            Console.WriteLine(JsonSerializer.Serialize(result));
            return;
        }

        var tags = new List<KeyValuePair<string, object?>> {
            KeyValuePair.Create<string,object?>("jobId", cmd.JobId),
            KeyValuePair.Create<string,object?>("taskId",cmd.TaskId),
            KeyValuePair.Create<string,object?>("jobAssembly",cmd.JobAssemblyPath),
            KeyValuePair.Create<string,object?>("jobClass",cmd.JobEntryClassName)
        };
        if (!string.IsNullOrEmpty(cmd.JobStartParam))
        {
            tags.Add(KeyValuePair.Create<string, object?>("param", cmd.JobStartParam));
        }
        tags.Add(KeyValuePair.Create<string, object?>("args", cmd.ToString()));
        var parent = new ActivityContext(ActivityTraceId.CreateFromString(cmd.TraceId), ActivitySpanId.CreateFromString(cmd.ParentSpanId), ActivityTraceFlags.Recorded);
        var activity = activitySource.StartActivity(ActivityKind.Internal, parent, name: "Shell Execute", tags: tags);
        activity?.SetTag("type", "scheduler-shell");
        try
        {
            Debug.WriteLine($"TraceId:{activity?.TraceId},spanId:{activity?.SpanId}");
            var jobContext = new JobContext() { TaskId = result.TaskId, JobId = jobId, ExcuteClassName = cmd.JobEntryClassName, ExecutionTime = excuteTime, ExcuteParameters = parameterArr == null ? new() : parameterArr.ToList() };

            await instance.InitializeAsync(service, jobId, result.TaskId);

            await instance.BeforeExcuteAsync(jobContext);

            var methodResult = await instance.ExcuteAsync(jobContext);

            jobContext.ExcuteResult = methodResult;

            await instance.AfterExcuteAsync(jobContext);

            result.IsSuccess = true;

            result.MethodResult = methodResult;

            result.Message = $"Run Job Success";
            activity?.SetStatus(Status.Ok);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(Status.Error);
            result.Message = $"Run Job Error, Exception Message: {ex.Message}";
            activity?.SetTag("exception.type", ex.GetType());
            activity?.SetTag("exception.message", ex.Message);
            activity?.SetTag("exception.stacktrace", ex.StackTrace);
        }
        finally
        {
            activity?.Stop();
            tracerProvider?.Dispose();
            activitySource.Dispose();
        }
        Console.WriteLine(JsonSerializer.Serialize(result));
    }
}
