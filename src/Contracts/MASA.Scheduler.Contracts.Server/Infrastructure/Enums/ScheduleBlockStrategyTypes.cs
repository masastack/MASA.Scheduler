﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Infrastructure.Enums
{
    public enum ScheduleBlockStrategyTypes
    {
        Serial = 1,
        Parallel,
        Throw,
        Cover
    }
}
