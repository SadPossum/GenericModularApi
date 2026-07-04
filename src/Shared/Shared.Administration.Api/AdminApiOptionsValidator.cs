namespace Shared.Administration.Api;

using Microsoft.Extensions.Options;
using Shared.Security;

internal sealed class AdminApiOptionsValidator : IValidateOptions<AdminApiOptions>
{
    public ValidateOptionsResult Validate(string? name, AdminApiOptions options)
    {
        if (!GmaClaimNames.IsValidClaimName(options.ActorIdClaim))
        {
            return ValidateOptionsResult.Fail(
                $"{AdminApiOptions.SectionName}:ActorIdClaim must be 1-{GmaClaimNames.MaxLength} characters and cannot contain whitespace or control characters.");
        }

        if (options.RequireTenantClaimMatch && string.IsNullOrWhiteSpace(options.TenantIdClaim))
        {
            return ValidateOptionsResult.Fail(
                $"{AdminApiOptions.SectionName}:TenantIdClaim is required when RequireTenantClaimMatch is true.");
        }

        if (!string.IsNullOrWhiteSpace(options.TenantIdClaim) &&
            !GmaClaimNames.IsValidClaimName(options.TenantIdClaim))
        {
            return ValidateOptionsResult.Fail(
                $"{AdminApiOptions.SectionName}:TenantIdClaim must be 1-{GmaClaimNames.MaxLength} characters and cannot contain whitespace or control characters.");
        }

        return ValidateOptionsResult.Success;
    }
}
