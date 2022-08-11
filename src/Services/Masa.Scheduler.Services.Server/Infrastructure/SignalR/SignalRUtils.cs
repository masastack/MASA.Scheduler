// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.SignalR;

public class SignalRUtils
{
    private readonly IHubContext<NotificationsHub> _hubContext;
    private readonly IDistributedCacheClient _distributedCacheClient;

    public SignalRUtils(IHubContext<NotificationsHub> hubContext, IDistributedCacheClient distributedCacheClient)
    {
        _hubContext = hubContext;
        _distributedCacheClient = distributedCacheClient;
    }

    public async Task SendNoticationByGroup(string group, string method, SchedulerTaskDto taskDto, int intervalSecond = 0)
    {
        if (intervalSecond > 0)
        {
            var key = $"{CacheKeys.SIGNALR_NOTIFY}-{group}-{method}";
            if (await _distributedCacheClient.ExistsAsync<bool>(key))
            {
                return;
            }
            else
            {
                _distributedCacheClient.Set(key, true, new CombinedCacheEntryOptions<bool>()
                {
                    DistributedCacheEntryOptions = new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(intervalSecond)
                    }
                });
            }
        }
        var groupClient = _hubContext.Clients.Groups(group);
        await groupClient.SendAsync(SignalRMethodConsts.GET_NOTIFICATION, taskDto);
    }

}
