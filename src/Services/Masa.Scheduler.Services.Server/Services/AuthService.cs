// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Services;

public class AuthService : ServiceBase
{
    public AuthService(IServiceCollection services) : base(services, ConstStrings.AUTH_API)
    {
        MapGet(GetTeamListAsync);
        MapGet(GetUserInfoAsync);
    }

    public async Task<IResult> GetTeamListAsync(IEventBus eventBus)
    {
        var query = new TeamQuery();
        await eventBus.PublishAsync(query);
        return Results.Ok(query.Result);
    }

    public async Task<IResult> GetUserInfoAsync(IEventBus eventBus, [FromQuery] Guid userId)
    {
        var query = new UserQuery();
        query.UserIds.Add(userId);
        await eventBus.PublishAsync(query);
        return Results.Ok(query.Result.FirstOrDefault() ?? new());
    }
}
