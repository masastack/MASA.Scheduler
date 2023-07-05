// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Infrastructure.Enums;

public enum TaskRunResultStatus
{
    Success = 3,
    Failure = 4,
    TimeoutSuccess = 6,
    Ignore = 9
}
