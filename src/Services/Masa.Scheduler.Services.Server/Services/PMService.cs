// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Services;

public class PMService : ServiceBase
{
    public PMService() : base(ConstStrings.PM_API)
    {
    }

    public async Task<IResult> GetProjectListAsync(IEventBus eventBus, Guid? teamId, string environment = "")
    {
        var query = new ProjectQuery(teamId, environment);
        await eventBus.PublishAsync(query);
        return Results.Ok(query.Result);
    }
}
