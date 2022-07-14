// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

var taskId = args[0];

var assemblyName = args[1];

var className = args[2];

Console.WriteLine($"Get args, TaskId: {taskId}, assemblyName: {assemblyName}, className: {className}");

Assembly assembly;

var result = new RunResult() { TaskId = new Guid(taskId) };

try
{
    assembly = Assembly.LoadFrom(assemblyName);
}
catch (Exception ex)
{
    result.Message = "Load Assembly Error, Exception Message: " + ex.Message;
    Console.WriteLine(JsonSerializer.Serialize(result));
    return;
}

var assemblyType = assembly.GetType(className);

if(assemblyType == null)
{
    result.Message = $"Assembly type not found, ClassName: {className}";
    Console.WriteLine(JsonSerializer.Serialize(result));
    return;
}

if (assemblyType.GetInterface(typeof(ISchedulerJob).Name) == null)
{
    result.Message = $"Class: {assemblyType.Name} not implement ISchedulerJob";
    Console.WriteLine(JsonSerializer.Serialize(result));
    return;
}

string[]? parameterArr = null;

if (args.Length >= 4)
{
    var parameter = args[3];
    Console.WriteLine($"Get parameter: {parameter}");
    parameterArr = parameter.Split(";");
}

var excuteTime = DateTimeOffset.Now;

if (args.Length >= 6)
{
    var tick = Convert.ToInt64(args[4]);
    var tickOffset = Convert.ToInt64(args[5]);

    DateTimeOffset parseExcuteTime = new DateTimeOffset(tick, new TimeSpan(tickOffset));

    Console.WriteLine($"parseExcuteTime: {parseExcuteTime}");

    if(parseExcuteTime != DateTimeOffset.MinValue)
    {
        excuteTime = parseExcuteTime;
    }
}

var jobId = Guid.Empty;
if (args.Length >= 7 && args[6] != null)
{
    jobId = new Guid(args[6].ToString());
}

var instance = Activator.CreateInstance(assemblyType) as ISchedulerJob;

if(instance == null)
{
    result.Message = $"Cannot create ISchedulerJob instance";
    Console.WriteLine(JsonSerializer.Serialize(result));
    return;
}

try
{
    var jobContext = new JobContext() { TaskId = result.TaskId, JobId = jobId, ExcuteClassName = className, ExecutionTime = excuteTime, ExcuteParameters = parameterArr == null ? new() : parameterArr.ToList() };

    await instance.BeforeExcuteAsync(jobContext);

    var methodResult = await instance.ExcuteAsync(jobContext);

    jobContext.ExcuteResult = methodResult;

    await instance.AfterExcuteAsync(jobContext);

    result.IsSuccess = true;

    result.MethodResult = methodResult;

    result.Message = $"Run Job Success";

    Console.WriteLine(JsonSerializer.Serialize(result));
}
catch(Exception ex)
{
    result.Message = $"Run Job Error, Exception Message: {ex.Message}";

    Console.WriteLine(JsonSerializer.Serialize(result));
}
