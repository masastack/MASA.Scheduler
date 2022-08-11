// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Auths;

public class AuthQueryHandler
{
    private IUserContext _userContext;
    private IAuthClient _authClient;
    private IMapper _mapper;
    private IMemoryCacheClient _memoryCacheClient;

    public AuthQueryHandler(IUserContext userContext, IAuthClient authClient, IMapper mapper, IMemoryCacheClient memoryCacheClient)
    {
        _userContext = userContext;
        _authClient = authClient;
        _mapper = mapper;
        _memoryCacheClient = memoryCacheClient;
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
        var md5Key = MD5Utils.Encrypt(EncryptType.Md5, JsonSerializer.Serialize(query.UserIds));

        var cacheKey = CacheKeys.USER_QUERY + "-" + md5Key;

        var response = await _memoryCacheClient.GetAsync<List<UserDto>>(cacheKey);

        if(response == null)
        {
            var userInfos = await _authClient.UserService.GetUserPortraitsAsync(query.UserIds.ToArray());

            if (userInfos.Any())
            {
                response = _mapper.Map<List<UserDto>>(userInfos);
            }

            await _memoryCacheClient.SetAsync(cacheKey, response ?? new(), new CombinedCacheEntryOptions<List<UserDto>>()
            {
                MemoryCacheEntryOptions = new MemoryCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                }
            });
        }

        query.Result = response ?? new();
    }
}
