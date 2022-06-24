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

string[]? parameterArr = null;

if (args.Length >= 4)
{
    var parameter = args[3];
    Console.WriteLine($"Get parameter: {parameter}");
    parameterArr = parameter.Split(";");
}

var instance = Activator.CreateInstance(assemblyType);

var method = assemblyType.GetMethod(methodName);

if(method == null)
{
    result.Message = $"Method not found, MethodName: {methodName}";
    Console.WriteLine(JsonSerializer.Serialize(result));
    return;
}

try
{
    var methodResult = method.Invoke(instance, parameterArr);

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
