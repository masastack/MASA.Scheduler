// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Validator;

public class SchedulerJobDtoValidator : AbstractValidator<SchedulerJobDto>
{
    public SchedulerJobDtoValidator()
    {
        RuleFor(job => job.FailedRetryCount).GreaterThanOrEqualTo(0);
        RuleFor(job => job.FailedRetryInterval).GreaterThanOrEqualTo(0);
        RuleFor(job => job.RunTimeoutSecond).GreaterThanOrEqualTo(0);
    }
}

