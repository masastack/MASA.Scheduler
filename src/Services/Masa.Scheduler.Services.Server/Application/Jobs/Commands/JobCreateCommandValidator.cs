// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Jobs.Commands
{
    public class JobCreateCommandValidator : AbstractValidator<JobCreateCommand>
    {
        public JobCreateCommandValidator()
        {
            //RuleFor(cmd => cmd.Items).Must(cmd => cmd.Any()).WithMessage("the order items cannot be empty");
        }
    }
}