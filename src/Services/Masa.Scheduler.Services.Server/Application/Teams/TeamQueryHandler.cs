﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Teams
{
    public class TeamQueryHandler
    {
        [EventHandler]
        public Task TeamListHandleAsync(TeamQuery query)
        {
            query.Result = new List<TeamModel>()
            {
                new TeamModel()
                {
                    Id = new Guid("22A8CA67-F5C2-44A7-2129-08DA1DE0EBC7"),
                    Name = "Masa团队",
                    Description = "Masa 团队",
                    MemberCount = 1,
                },
                new TeamModel()
                {
                    Id = new Guid("D0643F37-8202-45FF-D53C-08DA1D3F3845"),
                    Name = "IoT团队",
                    Description = "IoT 团队",
                    MemberCount =2,
                },
                new TeamModel()
                {
                    Id= new Guid("3119137C-DB47-4523-C509-08DA1EC03F6F"),
                    Name = "SEC团队",
                    Description ="SEC 团队",
                    MemberCount = 3,
                }
            };

            return Task.CompletedTask;

        }
    }
}
