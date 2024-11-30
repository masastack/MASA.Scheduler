// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Managers.Servers;

public class SchedulerServerManagerBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public SchedulerServerManagerBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var schedulerWorkerManager = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<SchedulerServerManager>();
        await schedulerWorkerManager.StartManagerAsync(stoppingToken);

        var environmentProvider = _serviceProvider.GetRequiredService<EnvironmentProvider>();

        foreach (var environment in environmentProvider.GetEnvionments())
        {
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
