// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Validator;

public class SchedulerJobDtoValidator : AbstractValidator<SchedulerJobDto>
{
    public SchedulerJobDtoValidator()
    {
        RuleFor(job => job.JobType).Required();
        RuleFor(job => job.ScheduleType).Required();
        RuleFor(job => job.RoutingStrategy).Required();
        RuleFor(job => job.ScheduleExpiredStrategy).Required();
        RuleFor(job => job.ScheduleBlockStrategy).Required();
        RuleFor(job => job.RunTimeoutStrategy).Required();
        RuleFor(job => job.Name).Required().MaxLength(100);
        RuleFor(job => job.Owner).Required().MaxLength(20);
        RuleFor(job => job.OwnerId).Required();
        RuleFor(job => job.CronExpression).MaxLength(100);
        RuleFor(job => job.FailedRetryCount).GreaterThanOrEqualTo(0);
        RuleFor(job => job.FailedRetryInterval).GreaterThanOrEqualTo(0);
        RuleFor(job => job.RunTimeoutSecond).GreaterThanOrEqualTo(0);
        RuleFor(job => job.Description).MaxLength(255);
    }
}
