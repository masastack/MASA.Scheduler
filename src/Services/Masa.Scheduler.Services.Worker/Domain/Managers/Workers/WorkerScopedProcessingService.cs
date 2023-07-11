// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Domain.Managers.Workers;

public class WorkerScopedProcessingService : IScopedProcessingService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMultiEnvironmentContext _multiEnvironmentContext;

    public WorkerScopedProcessingService(IServiceScopeFactory scopeFactory, IMultiEnvironmentContext multiEnvironmentContext)
    {
        _scopeFactory = scopeFactory;
        _multiEnvironmentContext = multiEnvironmentContext;
    }

    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        await ProcessTaskRun();
    }

    public Task ProcessTaskRun()
    {
        Task.Run(async () =>
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var multiEnvironmentSetter = scope.ServiceProvider.GetRequiredService<IMultiEnvironmentSetter>();
            multiEnvironmentSetter.SetEnvironment(_multiEnvironmentContext.CurrentEnvironment);
            var schedulerWorkerManager = scope.ServiceProvider.GetRequiredService<SchedulerWorkerManager>();

            while (true)
            {
                await schedulerWorkerManager.ProcessTaskRun();

                await Task.Delay(100);
            }
        });

        return Task.CompletedTask;
    }
}
