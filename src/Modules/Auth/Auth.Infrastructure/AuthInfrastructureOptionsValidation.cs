namespace Auth.Infrastructure;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

internal static class AuthInfrastructureOptionsValidation
{
    public static void Validate(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        JwtSettings jwtSettings = configuration
            .GetSection(JwtSettings.SectionName)
            .Get<JwtSettings>() ?? new JwtSettings();
        RefreshTokenHashingOptions refreshTokenHashingOptions = configuration
            .GetSection(RefreshTokenHashingOptions.SectionName)
            .Get<RefreshTokenHashingOptions>() ?? new RefreshTokenHashingOptions();

        ValidateJwtSettings(jwtSettings);
        ValidateRefreshTokenHashingOptions(refreshTokenHashingOptions);
    }

    private static void ValidateJwtSettings(JwtSettings options)
    {
        ValidateOptionsResult result = new JwtSettingsValidator().Validate(name: null, options);

        if (result.Failed)
        {
            throw new OptionsValidationException(JwtSettings.SectionName, typeof(JwtSettings), result.Failures);
        }
    }

    private static void ValidateRefreshTokenHashingOptions(RefreshTokenHashingOptions options)
    {
        ValidateOptionsResult result = new RefreshTokenHashingOptionsValidator().Validate(name: null, options);

        if (result.Failed)
        {
            throw new OptionsValidationException(
                RefreshTokenHashingOptions.SectionName,
                typeof(RefreshTokenHashingOptions),
                result.Failures);
        }
    }
}
