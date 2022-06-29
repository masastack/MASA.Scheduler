// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Auths.Queries;

public record UserQuery : Query<UserDto>
{
    public Guid UserId { get; set; }

    public override UserDto Result { get; set; } = new();
}
