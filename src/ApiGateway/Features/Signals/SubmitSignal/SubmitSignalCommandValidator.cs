namespace ApiGateway.Features.Signals.SubmitSignal;

using FluentValidation;

public class SubmitSignalCommandValidator : AbstractValidator<SubmitSignalCommand>
{
    public SubmitSignalCommandValidator()
    {
        RuleFor(x => x.SignalData)
            .NotEmpty().WithMessage("SignalData is required.")
            .Must(BeAnInteger).WithMessage("SignalData must be a valid integer.");
    }

    private bool BeAnInteger(string signalData)
    {
        return int.TryParse(signalData, out _);
    }
}
