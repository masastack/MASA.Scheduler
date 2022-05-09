// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Services
{
    public class AuthService : ServiceBase
    {
        public AuthService(IServiceCollection services) : base(services, "api/auth")
        {
            MapGet(TeamListAsync);
        }

        public async Task<IResult> TeamListAsync(IEventBus eventBus)
        {
            var query = new TeamQuery();
            await eventBus.PublishAsync(query);
            return Results.Ok(query.Result);
        }
    }
}
