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

    [RoutePattern("{name}", StartWithBaseUri = true, HttpMethod = "Get")]
    public async Task<IResult> GetAsync(DaprJobsClient daprJobsClient, string name, ILogger<DaprJobService> logger, CancellationToken cancellationToken = default)
    {
        try
        {
            var detail = await daprJobsClient.GetJobAsync(name, cancellationToken);
            return Results.Ok(detail);
        }
        catch (Exception ex) when (DaprJobsExceptionHelper.IsNotFound(ex))
        {
            logger.LogInformation("Dapr job not found. Name: {Name}", name);
            return Results.NotFound(new
            {
                Name = name,
                Message = "Dapr job was not found"
            });
        }
    }
}
