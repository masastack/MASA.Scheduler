// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Managers.Workers;

public class SchedulerWorkerManagerHostService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public SchedulerWorkerManagerHostService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var schedulerServerManager = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<SchedulerWorkerManager>();
        await schedulerServerManager.StartManagerAsync(stoppingToken);

        var environmentProvider = _serviceProvider.GetRequiredService<EnvironmentProvider>();
        var data = _serviceProvider.GetRequiredService<SchedulerWorkerManagerData>();
        foreach (var environment in environmentProvider.GetEnvionments())
        {
            data.TaskQueue.TryAdd(environment, new());

            await DoWorkAsync(environment, stoppingToken);
        }
    }

    private async Task DoWorkAsync(string environment, CancellationToken stoppingToken)
    {
        using (IServiceScope scope = _serviceProvider.CreateScope())
        {
            var multiEnvironmentSetter = scope.ServiceProvider.GetRequiredService<IMultiEnvironmentSetter>();
            multiEnvironmentSetter.SetEnvironment(environment);

            var scopedProcessingService =
                scope.ServiceProvider.GetRequiredService<IScopedProcessingService>();

            await scopedProcessingService.DoWorkAsync(stoppingToken);
        }
    }
}
