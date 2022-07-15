// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Authentication;

namespace Masa.Scheduler.Services.Server.Services;

public class AuthService : ServiceBase
{
    public AuthService(IServiceCollection services) : base(services, ConstStrings.AUTH_API)
    {
        MapGet(GetTeamListAsync);
    }

    public async Task<IResult> GetTeamListAsync(IEventBus eventBus, [FromServices] IUserContext userContext, [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        if(httpContextAccessor.HttpContext != null)
        {
            var token = httpContextAccessor.HttpContext.GetTokenAsync("Bearer");

        }

        var userId = userContext.GetUserId<Guid>();

        var query = new TeamQuery();
        await eventBus.PublishAsync(query);
        return Results.Ok(query.Result);
    }
}
