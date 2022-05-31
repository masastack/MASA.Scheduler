// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Validator;

internal class SchedulerJobHttpConfigDtoValidator : AbstractValidator<SchedulerJobHttpConfigDto>
{
    public SchedulerJobHttpConfigDtoValidator()
    {
        RuleFor(config => config.HttpMethod).Required();
        RuleFor(config => config.RequestUrl).Required();
    }
}

