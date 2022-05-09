namespace MASA.Scheduler.Service.Application.Jobs.Commands;

public class JobCreateCommandValidator : AbstractValidator<JobCreateCommand>
{
    public JobCreateCommandValidator()
    {
        //RuleFor(cmd => cmd.Items).Must(cmd => cmd.Any()).WithMessage("the order items cannot be empty");
    }
}
