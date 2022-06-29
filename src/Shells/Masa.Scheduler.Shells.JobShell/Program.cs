// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

var taskId = args[0];

var assemblyName = args[1];

var entryMethod = args[2];

Console.WriteLine($"Get args, TaskId: {taskId}, assemblyName: {assemblyName}, entryMethod: {entryMethod}");

var lastIndex = entryMethod.LastIndexOf('.');

var className = entryMethod.Substring(0, lastIndex);

var methodName = entryMethod.Substring(lastIndex + 1);

var path = Path.Combine(Environment.CurrentDirectory, assemblyName);

Assembly assembly;

var result = new RunResult() { TaskId = new Guid(taskId) };

try
{
    assembly = Assembly.LoadFrom(path);
}
catch (Exception ex)
{
    result.Message = "Load Assembly Error, Exception Message: " + ex.Message;
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

if (args.Length >= 5)
{
    DateTimeOffset parseExcuteTime = Convert.ToDateTime(args[4]);

    if(parseExcuteTime != DateTimeOffset.MinValue)
    {
        excuteTime = parseExcuteTime;
    }
}

var jobId = Guid.Empty;

if(args.Length >= 6 && args[5] != null)
{
    jobId = new Guid(args[5].ToString());
}

var instance = Activator.CreateInstance(assemblyType) as ISchedulerJob;

if(instance == null)
{
    result.Message = $"Cannot create ISchedulerJob instance";
    Console.WriteLine(JsonSerializer.Serialize(result));
    return;
}

var method = assemblyType.GetMethod(methodName);

if(method == null)
{
    result.Message = $"Method not found, MethodName: {methodName}";
    Console.WriteLine(JsonSerializer.Serialize(result));
    return;
}

try
{
    var jobContext = new JobContext() { TaskId = result.TaskId, JobId = jobId, ExcuteMethodName = methodName, ExecutionTime = excuteTime, ExcuteParameters = parameterArr == null ? new() : parameterArr.ToList() };

    await instance.BeforeExcuteAsync(jobContext);

    var methodResult = method.Invoke(instance, parameterArr);

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
