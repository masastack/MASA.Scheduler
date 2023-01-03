// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Validator;

public class SchedulerJobAppConfigDtoValidator: AbstractValidator<SchedulerJobAppConfigDto>
{
    public SchedulerJobAppConfigDtoValidator()
    {
        RuleFor(config => config.JobAppIdentity).Required();
        RuleFor(config => config.JobEntryAssembly).Required();
        RuleFor(config => config.JobEntryClassName).Required();
    }
}

