namespace Shared.Infrastructure.Messaging;

using Microsoft.Extensions.Options;

public sealed class NatsJetStreamOptionsValidator : IValidateOptions<NatsJetStreamOptions>
{
    public ValidateOptionsResult Validate(string? name, NatsJetStreamOptions options)
    {
        return NatsStreamNames.IsValid(options.StreamName)
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(
                $"{NatsJetStreamOptions.SectionName}:StreamName must be 1-{NatsStreamNames.MaxLength} characters and use only ASCII letters, digits, '-' or '_'.");
    }
}
