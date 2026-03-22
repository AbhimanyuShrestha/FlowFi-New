using FluentValidation;

namespace FlowFi.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Must contain at least one uppercase letter")
            .Matches("[0-9]").WithMessage("Must contain at least one number");
        RuleFor(x => x.FullName).MaximumLength(100).When(x => x.FullName is not null);
        RuleFor(x => x.Currency).Length(3).When(x => x.Currency is not null);
    }
}
