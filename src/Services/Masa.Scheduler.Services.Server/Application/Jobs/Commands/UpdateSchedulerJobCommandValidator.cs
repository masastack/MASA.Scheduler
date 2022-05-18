// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Jobs.Commands;

public class UpdateSchedulerJobCommandValidator: AbstractValidator<UpdateSchedulerJobCommand>
{
    public UpdateSchedulerJobCommandValidator()
    {
        RuleFor(command => command.Request.Data).SetValidator(new SchedulerJobDtoValidator());
    }
}
