// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Validator;

public class SchedulerJobValidator<T> : AbstractValidator<T> where T : SchedulerJobDto
{
    public SchedulerJobValidator()
    {
        RuleFor(job => job.FailedRetryCount).GreaterThanOrEqualTo(0);
        RuleFor(job => job.FailedRetryInterval).GreaterThanOrEqualTo(0);
        RuleFor(job => job.RunTimeoutSecond).GreaterThanOrEqualTo(0);
    }
}

