// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Validator;

public class SchedulerJobDaprServiceInvocationConfigDtoValidator: AbstractValidator<SchedulerJobDaprServiceInvocationConfigDto>
{
    public SchedulerJobDaprServiceInvocationConfigDtoValidator()
    {
        RuleFor(config => config.MethodName).Required();
        RuleFor(config => config.HttpMethod).Required();
        RuleFor(config => config.DaprServiceIdentity).Required();
    }
}

