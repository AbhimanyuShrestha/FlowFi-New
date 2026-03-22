using FluentValidation;

namespace FlowFi.Application.Features.Transactions.Commands.CreateTransaction;

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Description).MaximumLength(255).When(x => x.Description is not null);
        RuleFor(x => x.IdempotencyKey).MaximumLength(100).When(x => x.IdempotencyKey is not null);
    }
}
