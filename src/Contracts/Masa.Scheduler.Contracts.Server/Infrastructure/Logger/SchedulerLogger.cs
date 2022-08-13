// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Infrastructure.Logger;

public class SchedulerLogger
{
    private readonly ILogger _logger;

    public const string LOGGER_BODY = "LogType: {LogType}, Writer: {Writer}, TaskId: {TaskId}, JobId: {JobId}, Message: {Message}";

    public SchedulerLogger(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(LoggerTypes.SchedulerInternalLog.ToString());
    }

    public void LogInformation(string message, WriterTypes writerType, Guid taskId, Guid jobId)
    {
        _logger.LogInformation(LOGGER_BODY, LoggerTypes.SchedulerInternalLog.ToString(), writerType.ToString(), taskId, jobId, message);
    }

    public void LogError(Exception exception, string message, WriterTypes writerType, Guid taskId, Guid jobId)
    {
        _logger.LogError(exception, LOGGER_BODY, LoggerTypes.SchedulerInternalLog.ToString(), writerType.ToString(), taskId, jobId, message);
    }

    public void LogError(string message, WriterTypes writerType, Guid taskId, Guid jobId)
    {
        _logger.LogError(LOGGER_BODY, LoggerTypes.SchedulerInternalLog.ToString(), writerType.ToString(), taskId, jobId, message);
    }

    public void LogWarning(string message, WriterTypes writerType, Guid taskId, Guid jobId)
    {
        _logger.LogWarning(LOGGER_BODY, LoggerTypes.SchedulerInternalLog.ToString(), writerType.ToString(), taskId, jobId, message);
    }

    public void LogDebug(string message, WriterTypes writerType, Guid taskId, Guid jobId)
    {
        _logger.LogDebug(LOGGER_BODY, LoggerTypes.SchedulerInternalLog.ToString(), writerType.ToString(), taskId, jobId, message);
    }
}
