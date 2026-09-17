// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using SchedulerHttpMethods = Masa.Scheduler.Contracts.Server.Infrastructure.Enums.HttpMethods;

namespace Masa.Scheduler.Services.Server.Infrastructure.Extensions;

public static class AuthSchedulerJobSeedDataExtensions
{
    public static async Task SeedAuthSchedulerJobsAsync(this IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var authUrl = scope.ServiceProvider.GetRequiredService<IMasaStackConfig>().GetAuthServiceDomain().TrimEnd('/');

        foreach (var request in GetAuthSchedulerJobs(authUrl))
        {
            await serviceProvider.SafeExecuteAsync(request);
        }
    }

    private static IEnumerable<AddSchedulerJobBySdkRequest> GetAuthSchedulerJobs(string authUrl)
    {
        yield return CreateHttpJob(
            "masa-auth-sync-userAutoComplete-job",
            "SyncUserAutoCompleteJob",
            $"{authUrl}/api/user/SyncUserAutoComplete/",
            12 * 60 * 60,
            "{\"OnceExecuteCount\":1000}");

        yield return CreateHttpJob(
            "masa-auth-sync-syncUserRedis-job",
            "SyncUserRedisJob",
            $"{authUrl}/api/user/SyncRedis/",
            12 * 60 * 60);

        yield return CreateHttpJob(
            "masa-auth-sync-syncOidcRedis-job",
            "SyncOidcRedisJob",
            $"{authUrl}/api/sso/client/SyncOidc/",
            12 * 60 * 60,
            description: "SyncUserRedisJob");

        yield return CreateHttpJob(
            "masa-auth-sync-syncPermissionRedis-job",
            "SyncPermissionRedisJob",
            $"{authUrl}/api/permission/SyncRedis",
            12 * 60 * 60);

        yield return CreateHttpJob(
            "masa-auth-sync-user-from-ldap-job",
            "SyncUserFromLdapJob",
            $"{authUrl}/api/user/SyncFromLdap",
            12 * 60,
            httpMethod: SchedulerHttpMethods.GET);
    }

    private static AddSchedulerJobBySdkRequest CreateHttpJob(
        string jobIdentity,
        string name,
        string requestUrl,
        int runTimeoutSecond,
        string httpBody = "",
        SchedulerHttpMethods httpMethod = SchedulerHttpMethods.POST,
        string description = "")
    {
        return new AddSchedulerJobBySdkRequest
        {
            ProjectIdentity = MasaStackProject.Auth.Name,
            JobIdentity = jobIdentity,
            Name = name,
            IsAlertException = true,
            JobType = JobTypes.Http,
            CronExpression = "0 0 0 * * ? *",
            Description = string.IsNullOrEmpty(description) ? name : description,
            ScheduleExpiredStrategy = ScheduleExpiredStrategyTypes.ExecuteImmediately,
            ScheduleBlockStrategy = ScheduleBlockStrategyTypes.Cover,
            RunTimeoutStrategy = RunTimeoutStrategyTypes.RunFailedStrategy,
            RunTimeoutSecond = runTimeoutSecond,
            FailedRetryInterval = 10,
            FailedRetryCount = 3,
            HttpConfig = new SchedulerJobHttpConfigDto
            {
                HttpMethod = httpMethod,
                RequestUrl = requestUrl,
                HttpBody = httpBody
            }
        };
    }

    private static async Task SafeExecuteAsync(this IServiceProvider serviceProvider, AddSchedulerJobBySdkRequest request)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var scopedServiceProvider = scope.ServiceProvider;
            var schedulerJobRepository = scopedServiceProvider.GetRequiredService<ISchedulerJobRepository>();
            var job = await schedulerJobRepository.FindAsync(job =>
                job.JobIdentity == request.JobIdentity &&
                job.BelongProjectIdentity == request.ProjectIdentity);

            if (job != null)
            {
                return;
            }

            var command = new AddSchedulerJobBySdkCommand(request);
            await scopedServiceProvider.GetRequiredService<IEventBus>().PublishAsync(command);
        }
        catch (Exception ex)
        {
            try
            {
                if (await serviceProvider.SchedulerJobExistsAsync(request))
                {
                    return;
                }
            }
            catch (Exception existsException)
            {
                logger.LogWarning(existsException, "Check seeded scheduler job {JobIdentity} failed", request.JobIdentity);
            }

            logger.LogError(ex, "Seed scheduler job {JobIdentity} failed", request.JobIdentity);
        }
    }

    private static async Task<bool> SchedulerJobExistsAsync(this IServiceProvider serviceProvider, AddSchedulerJobBySdkRequest request)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var schedulerJobRepository = scope.ServiceProvider.GetRequiredService<ISchedulerJobRepository>();
        var job = await schedulerJobRepository.FindAsync(job =>
            job.JobIdentity == request.JobIdentity &&
            job.BelongProjectIdentity == request.ProjectIdentity);

        return job != null;
    }
}
