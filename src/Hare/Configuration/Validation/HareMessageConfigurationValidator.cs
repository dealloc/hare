using Microsoft.Extensions.Options;

namespace Hare.Configuration.Validation;

/// <summary>
/// Validate the <see cref="HareMessageConfiguration{TMessage}" /> options for <typeparamref name="TMessage" />.
/// </summary>
public class HareMessageConfigurationValidator<TMessage> : IValidateOptions<HareMessageConfiguration<TMessage>>
    where TMessage : class
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, HareMessageConfiguration<TMessage> options)
    {
        List<string> errors = [];

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (options.JsonTypeInfo is null)
            errors.Add($"{nameof(options.JsonTypeInfo)} can not be null.");

        if (string.IsNullOrWhiteSpace(options.QueueName))
            errors.Add($"{nameof(options.QueueName)} can not be null or empty.");

        if (string.IsNullOrWhiteSpace(options.DeadLetterExchange) &&
            !string.IsNullOrWhiteSpace(options.DeadLetterQueueName))
            errors.Add(
                $"{nameof(options.DeadLetterExchange)} can not be null or empty when ${nameof(options.DeadLetterExchange)} is set.)");
        else if (!string.IsNullOrWhiteSpace(options.DeadLetterExchange) &&
                 string.IsNullOrWhiteSpace(options.DeadLetterQueueName))
            errors.Add(
                $"{nameof(options.DeadLetterQueueName)} can not be null or empty when ${nameof(options.DeadLetterExchange)} is set.)");

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}