// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Infrastructure.Logger;

public class SchedulerLogger
{
    private readonly ILogger _logger;

    public const string LOGGER_BODY = "{Message}, LogType: {LogType}, LogWriter: {LogWriter}, TaskId: {TaskId}, JobId: {JobId}";

    public SchedulerLogger(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(LoggerTypes.SchedulerInternalLog.ToString());
    }

    public void LogInformation(string message, WriterTypes writerType, Guid taskId, Guid jobId, LoggerTypes loggerType = LoggerTypes.SchedulerInternalLog)
    {
        _logger.LogInformation(LOGGER_BODY, message, loggerType.ToString(), writerType.ToString(), taskId, jobId);
    }

    public void LogError(Exception exception, string message, WriterTypes writerType, Guid taskId, Guid jobId, LoggerTypes loggerType = LoggerTypes.SchedulerInternalLog)
    {
        _logger.LogError(exception, LOGGER_BODY, message, loggerType.ToString(), writerType.ToString(), taskId, jobId);
    }

    public void LogError(string message, WriterTypes writerType, Guid taskId, Guid jobId, LoggerTypes loggerType = LoggerTypes.SchedulerInternalLog)
    {
        _logger.LogError(LOGGER_BODY, message, loggerType.ToString(), writerType.ToString(), taskId, jobId);
    }

    public void LogWarning(string message, WriterTypes writerType, Guid taskId, Guid jobId, LoggerTypes loggerType = LoggerTypes.SchedulerInternalLog)
    {
        _logger.LogWarning(LOGGER_BODY, message, loggerType.ToString(), writerType.ToString(), taskId, jobId);
    }

    public void LogDebug(string message, WriterTypes writerType, Guid taskId, Guid jobId, LoggerTypes loggerType = LoggerTypes.SchedulerInternalLog)
    {
        _logger.LogDebug(LOGGER_BODY, message, loggerType.ToString(), writerType.ToString(), taskId, jobId);
    }
}
