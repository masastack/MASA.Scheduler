namespace Masa.Scheduler.Services.Server.Application.Resources.Commands;

public class UpdateSchedulerResourceCommandValidator : AbstractValidator<UpdateSchedulerResourceCommand>
{
    public UpdateSchedulerResourceCommandValidator() => RuleFor(cmd => cmd.Request.Data).SetValidator(new SchedulerResourceDtoValidator());
}