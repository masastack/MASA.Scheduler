// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.BuildingBlocks.StackSdks.Auth.Contracts;
using Masa.BuildingBlocks.StackSdks.Config;

namespace Masa.Scheduler.Services.Server.Infrastructure.Middleware;

public class DisabledCommandMiddleware<TEvent> : Middleware<TEvent>
    where TEvent : notnull, IEvent
{
    readonly ILogger<DisabledCommandMiddleware<TEvent>> _logger;
    readonly IUserContext _userContext;
    readonly IMasaStackConfig _masaStackConfig;

    public DisabledCommandMiddleware(
        ILogger<DisabledCommandMiddleware<TEvent>> logger,
        IUserContext userContext,
        IMasaStackConfig masaStackConfig)
    {
        _logger = logger;
        _userContext = userContext;
        _masaStackConfig = masaStackConfig;
    }

    public override async Task HandleAsync(TEvent @event, EventHandlerDelegate next)
    {
        var user = _userContext.GetUser<MasaUser>();
        if (_masaStackConfig.IsDemo && user?.Account?.ToLower() == "guest" && @event is ICommand)
        {
            _logger.LogWarning("Guest operation");
            throw new UserFriendlyException("演示账号禁止操作");
        }
        await next();
    }
}
