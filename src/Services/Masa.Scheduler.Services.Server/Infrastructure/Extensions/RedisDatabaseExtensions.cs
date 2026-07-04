// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Extensions;

public static class RedisDatabaseExtensions
{
    public static async Task<bool> SetAddAndListRightPushWhenNewAsync(this IDatabase redis, RedisKey listKey, RedisKey setKey, RedisValue value)
    {
        var result = await redis.ScriptEvaluateAsync(RedisLuaScripts.SetAddAndListRightPushWhenNew, [listKey, setKey], [value]);
        return (long)result == 1;
    }

    public static async Task<RedisValue> ListLeftPopAndSetRemoveAsync(this IDatabase redis, RedisKey listKey, RedisKey setKey)
    {
        var result = await redis.ScriptEvaluateAsync(RedisLuaScripts.ListLeftPopAndSetRemove, [listKey, setKey]);
        return result.IsNull ? RedisValue.Null : (RedisValue)result.ToString();
    }
}
