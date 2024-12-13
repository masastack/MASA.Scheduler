// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Managers.Servers;

public class SchedulerServerManagerBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMasaConfiguration _configuration;
    private readonly ILogger<SchedulerServerManagerBackgroundService> _logger;

    public SchedulerServerManagerBackgroundService(IServiceProvider serviceProvider, IMasaConfiguration configuration, ILogger<SchedulerServerManagerBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var schedulerWorkerManager = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<SchedulerServerManager>();
        await schedulerWorkerManager.StartManagerAsync(stoppingToken);

        var environmentProvider = _serviceProvider.GetRequiredService<EnvironmentProvider>();
        var allowedEnvironments = GetAllowedEnvironments();

        foreach (var environment in environmentProvider.GetEnvionments())
        {
            if (!IsEnvironmentAllowed(environment, allowedEnvironments))
            {
                continue;
            }

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

            try
            {
                await scopedProcessingService.DoWorkAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while processing environment: {environment}.");
            }
        }
    }

    private List<string> GetAllowedEnvironments()
    {
        var configuration = _configuration.ConfigurationApi.GetDefault();
        return configuration.GetSection("AppSettings:AllowedEnvironments")
                            .Get<List<string>>() ?? new();
    }

    private bool IsEnvironmentAllowed(string environment, List<string> allowedEnvironments)
    {
        return !allowedEnvironments.Any() || allowedEnvironments.Contains(environment, StringComparer.OrdinalIgnoreCase);
    }
}
