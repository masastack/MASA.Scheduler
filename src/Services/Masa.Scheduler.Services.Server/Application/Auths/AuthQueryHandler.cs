// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.Scheduler.Services.Server.Application.Auths.Queries;

namespace Masa.Scheduler.Services.Server.Application.Auths;

public class AuthQueryHandler
{
    [EventHandler]
    public Task TeamListHandleAsync(TeamQuery query)
    {
        query.Result = new List<TeamDto>()
        {
            new TeamDto()
            {
                Id = new Guid("00000000-0000-0000-0000-000000000000"),
                Name = "默认团队",
                Description = "默认团队",
                MemberCount = 1,
            },
            new TeamDto()
            {
                Id = new Guid("713334DC-F91E-4ADA-9B16-C2D0881DC2F2"),
                Name = "Masa团队",
                Description = "Masa团队",
                MemberCount =2,
            },
            new TeamDto()
            {
                Id= new Guid("3119137C-DB47-4523-C509-08DA1EC03F6F"),
                Name = "SEC团队",
                Description ="SEC 团队",
                MemberCount = 3,
            }
        };

        return Task.CompletedTask;
    }

    [EventHandler]
    public Task GetUserAsync(UserQuery query)
    {
        query.Result = new UserDto()
        {
            Account = "Tester",
            Id = Guid.Empty,
            Name = "Tester"
        };

        return Task.CompletedTask;
    }
}
