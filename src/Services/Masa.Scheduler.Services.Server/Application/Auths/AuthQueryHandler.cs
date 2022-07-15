// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.BuildingBlocks.BasicAbility.Auth;
using Masa.Scheduler.Services.Server.Application.Auths.Queries;

namespace Masa.Scheduler.Services.Server.Application.Auths;

public class AuthQueryHandler
{
    private IUserContext _userContext;
    private IAuthClient _authClient;

    public AuthQueryHandler(IUserContext userContext, IAuthClient authClient)
    {
        _userContext = userContext;
        _authClient = authClient;
    }

    [EventHandler]
    public async Task TeamListHandleAsync(TeamQuery query)
    {
        //var currentUserId = _userContext.GetUserId<Guid>();

        //var teamList = await _authClient.TeamService.GetUserTeamsAsync();

        //var userInfos = await _authClient.UserService.GetUserPortraitsAsync(new Guid[] { currentUserId });

        //query.Result = teamList.Select(p => new TeamDto()
        //{
        //    Id = p.Id,
        //    Name = p.Name,
        //    Description = p.Description,
        //    Avatar = p.Avatar,
        //    MemberCount = p.MemberCount

        //}).ToList();

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

        //return Task.CompletedTask;
    }

    [EventHandler]
    public async Task GetUserAsync(UserQuery query)
    {
        //var userInfos = await _authClient.UserService.GetUserPortraitsAsync(query.UserId);

        //var userInfo = userInfos.FirstOrDefault();

        //if (userInfo != null)
        //{
        //    var useDto = new UserDto()
        //    {
        //        Account = userInfo.Account,
        //        Id = userInfo.Id,
        //        Name = userInfo.Name,
        //        Avatar = userInfo.Avatar,
        //        DisplayName = userInfo.DisplayName
        //    };

        //    query.Result = useDto;
        //}

        query.Result = new UserDto()
        {
            Account = "Tester",
            Id = Guid.Empty,
            Name = "Tester"
        };

        //return Task.CompletedTask;
    }
}
