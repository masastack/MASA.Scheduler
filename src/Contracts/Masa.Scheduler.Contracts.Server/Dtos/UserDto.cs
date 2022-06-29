// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Dtos;

public class UserDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public string Account { get; set; } = string.Empty;

    public int Gender { get; set; }

    public string Avatar { get; set; } = string.Empty;

    public string? IdCard { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? CompanyName { get; set; }

    public string? Department { get; set; }

    public string? Position { get; set; }
}

