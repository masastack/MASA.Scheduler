// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Services;

public class DaprJobService : ServiceBase
{
    public DaprJobService() : base(ConstStrings.SCHEDULER_DAPR_JOB_API)
    {
        RouteHandlerBuilder = builder =>
        {
            builder.RequireAuthorization();
        };
    }

    [RoutePattern("cron/{jobId}", StartWithBaseUri = true, HttpMethod = "Get")]
    public async Task<IResult> GetCronAsync(
        DaprJobsClient daprJobsClient,
        IMultiEnvironmentContext multiEnvironmentContext,
        Guid jobId,
        ILogger<DaprJobService> logger,
        CancellationToken cancellationToken = default)
    {
        var name = DaprJobsNameHelper.BuildCronName(multiEnvironmentContext.CurrentEnvironment, jobId);
        try
        {
            var detail = await daprJobsClient.GetJobAsync(name, cancellationToken);
            return Results.Ok(detail);
        }
        catch (Exception ex) when (DaprJobsExceptionHelper.IsNotFound(ex))
        {
            logger.LogInformation("Dapr cron job not found. JobId: {JobId}, Name: {Name}", jobId, name);
            return Results.NotFound(new
            {
                JobId = jobId,
                Name = name,
                Message = "Dapr cron job was not found"
            });
        }
    }

    [RoutePattern("retry/{jobId}/{taskId}", StartWithBaseUri = true, HttpMethod = "Get")]
    public async Task<IResult> GetRetryAsync(
        DaprJobsClient daprJobsClient,
        IMultiEnvironmentContext multiEnvironmentContext,
        Guid jobId,
        Guid taskId,
        ILogger<DaprJobService> logger,
        CancellationToken cancellationToken = default)
    {
        var name = DaprJobsNameHelper.BuildRetryName(multiEnvironmentContext.CurrentEnvironment, jobId, taskId);
        try
        {
            var detail = await daprJobsClient.GetJobAsync(name, cancellationToken);
            return Results.Ok(detail);
        }
        catch (Exception ex) when (DaprJobsExceptionHelper.IsNotFound(ex))
        {
            logger.LogInformation("Dapr retry job not found. JobId: {JobId}, TaskId: {TaskId}, Name: {Name}", jobId, taskId, name);
            return Results.NotFound(new
            {
                JobId = jobId,
                TaskId = taskId,
                Name = name,
                Message = "Dapr retry job was not found"
            });
        }
    }

}
