using FluentValidation;
using Mediator;

namespace Application.Behaviors;

internal sealed class ValidatePipelineBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private readonly IEnumerable<IValidator<TMessage>> _validators;

    public ValidatePipelineBehavior(IEnumerable<IValidator<TMessage>> validators)
    {
        _validators = validators;
    }

    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        var validators = _validators.ToArray();

        if (validators.Length == 0)
            return await next(message, cancellationToken);

        var context = new ValidationContext<TMessage>(message);

        var validationResults = await Task.WhenAll(
            validators.Select(validator =>
                validator.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToArray();

        if (failures.Length > 0)
            throw new ValidationException(failures);

        return await next(message, cancellationToken);
    }
}