// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Auths;

public class AuthQueryHandler
{
    private IUserContext _userContext;
    private IAuthClient _authClient;
    private IMapper _mapper;

    public AuthQueryHandler(IUserContext userContext, IAuthClient authClient, IMapper mapper)
    {
        _userContext = userContext;
        _authClient = authClient;
        _mapper = mapper;
    }

    [EventHandler]
    public async Task TeamListHandleAsync(TeamQuery query)
    {
        var teamList = await _authClient.TeamService.GetUserTeamsAsync();

        query.Result = teamList.Select(p => new TeamDto()
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Avatar = p.Avatar,
            MemberCount = p.MemberCount
        }).ToList();
    }

    [EventHandler]
    public async Task GetUserAsync(UserQuery query)
    {
        var userInfos = await _authClient.UserService.GetListByIdsAsync(query.UserIds.ToArray()) ?? new();

        var response = _mapper.Map<List<UserDto>>(userInfos);

        query.Result = response ?? new();
    }
}
