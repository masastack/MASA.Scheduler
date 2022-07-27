// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Auths.Queries;

public record UserQuery : Query<List<UserDto>>
{
    public List<Guid> UserIds { get; set; } = new();

    public override List<UserDto> Result { get; set; } = new();
}
