namespace Shared.Infrastructure.Caching;

using Microsoft.Extensions.Options;

internal sealed class CachingCompositionOptionsValidator(IServiceProvider serviceProvider)
    : IValidateOptions<CachingOptions>
{
    public ValidateOptionsResult Validate(string? name, CachingOptions options)
    {
        try
        {
            _ = CachingCompositionGuard.EnsureValid(options, serviceProvider);
            return ValidateOptionsResult.Success;
        }
        catch (InvalidOperationException exception)
        {
            return ValidateOptionsResult.Fail(exception.Message);
        }
    }
}
