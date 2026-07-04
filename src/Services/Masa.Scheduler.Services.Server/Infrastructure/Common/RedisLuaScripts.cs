// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Common;

public static class RedisLuaScripts
{
    public const string SetAddAndListRightPushWhenNew = """
        local added = redis.call('SADD', KEYS[2], ARGV[1])
        if added == 1 then
            redis.call('RPUSH', KEYS[1], ARGV[1])
        end
        return added
        """;

    public const string ListLeftPopAndSetRemove = """
        local value = redis.call('LPOP', KEYS[1])
        if value then
            redis.call('SREM', KEYS[2], value)
        end
        return value
        """;
}
