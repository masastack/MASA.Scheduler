// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.SignalR;

public class SignalRUtils
{
    private readonly IHubContext<NotificationsHub> _hubContext;
    private readonly IDistributedCacheClient _distributedCacheClient;
    private readonly IMultiEnvironmentContext _multiEnvironmentContext;

    public SignalRUtils(IHubContext<NotificationsHub> hubContext, IDistributedCacheClient distributedCacheClient, IMultiEnvironmentContext multiEnvironmentContext)
    {
        _hubContext = hubContext;
        _distributedCacheClient = distributedCacheClient;
        _multiEnvironmentContext = multiEnvironmentContext;
    }

    public async Task SendNoticationByGroup(string group, string method, SchedulerTaskDto taskDto, int intervalSecond = 0)
    {
        if (intervalSecond > 0)
        {
            var key = $"{CacheKeys.SIGNALR_NOTIFY}-{group}-{method}";
            if (await _distributedCacheClient.ExistsAsync(key))
            {
                return;
            }
            else
            {
                await _distributedCacheClient.SetAsync(key, true, TimeSpan.FromSeconds(intervalSecond));
            }
        }
        var groupClient = _hubContext.Clients.Groups(group);
        await groupClient.SendAsync(SignalRMethodConsts.GET_NOTIFICATION, taskDto, _multiEnvironmentContext.CurrentEnvironment);
    }

}
