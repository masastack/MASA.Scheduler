// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Validator;

public class SchedulerResourceDtoValidator : AbstractValidator<SchedulerResourceDto>
{
    public SchedulerResourceDtoValidator()
    {
        RuleFor(x=> x.FilePath).Required();
        RuleFor(x=> x.Name).Required();
        RuleFor(x => x.JobAppIdentity).Required();
        RuleFor(x => x.Version).Required();
    }
}

