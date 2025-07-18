namespace Masa.Scheduler.Services.Server.Application.Resources.Commands;

public class AddSchedulerResourceCommandValidator : AbstractValidator<AddSchedulerResourceCommand>
{
    public AddSchedulerResourceCommandValidator() => RuleFor(cmd => cmd.Request.Data).SetValidator(new SchedulerResourceDtoValidator());
}