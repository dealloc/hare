using Microsoft.Extensions.Options;

namespace Hare.Configuration.Validation;

public class HareMessageConfigurationValidator<TMessage> : IValidateOptions<HareMessageConfiguration<TMessage>>
{
    public ValidateOptionsResult Validate(string? name, HareMessageConfiguration<TMessage> options)
    {
        var errors = new List<string>();

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (options.JsonTypeInfo is null)
            errors.Add($"{nameof(options.JsonTypeInfo)} can not be null.");

        if (string.IsNullOrWhiteSpace(options.QueueName))
            errors.Add($"{nameof(options.QueueName)} can not be null or empty.");

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}