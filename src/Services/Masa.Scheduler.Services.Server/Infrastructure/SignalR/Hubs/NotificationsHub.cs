// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.SignalR.Hubs;

public class NotificationsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        await Groups.AddToGroupAsync(Context.ConnectionId, ConstStrings.GLOBAL_GROUP, Context.ConnectionAborted);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ConstStrings.GLOBAL_GROUP, Context.ConnectionAborted);
    }
}
