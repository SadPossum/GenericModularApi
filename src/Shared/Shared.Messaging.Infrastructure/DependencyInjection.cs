namespace Shared.Messaging.Infrastructure;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shared.Messaging;
using Shared.Observability.Infrastructure;
using Shared.Runtime.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddMessagingInfrastructure(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        ValidateOutboxOptions(builder.Configuration);

        builder.AddRuntimeInfrastructure();

        if (builder.Services.Any(descriptor => descriptor.ServiceType == typeof(MessagingInfrastructureRegistrationMarker)))
        {
            return builder;
        }

        builder.Services.AddSingleton<MessagingInfrastructureRegistrationMarker>();
        builder.Services
            .AddOptions<OutboxOptions>()
            .Bind(builder.Configuration.GetSection(OutboxOptions.SectionName))
            .ValidateOnStart();
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<OutboxOptions>, OutboxOptionsValidator>());
        builder.Services.TryAddSingleton<OutboxMetrics>();
        builder.Services.TryAddSingleton<InboxMetrics>();
        builder.Services.TryAddScoped<IOutboxWriterRegistry, OutboxWriterRegistry>();
        builder.Services.TryAddSingleton<IEventBus, NullEventBus>();

        return builder;
    }

    public static IHostApplicationBuilder AddOutboxPublishing(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddMessagingInfrastructure();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, OutboxPublisherService>());
        return builder;
    }

    private static void ValidateOutboxOptions(IConfiguration configuration)
    {
        OutboxOptions options = configuration
            .GetSection(OutboxOptions.SectionName)
            .Get<OutboxOptions>() ?? new OutboxOptions();
        ValidateOptionsResult result = new OutboxOptionsValidator().Validate(name: null, options);

        if (result.Failed)
        {
            throw new OptionsValidationException(OutboxOptions.SectionName, typeof(OutboxOptions), result.Failures);
        }
    }

    private sealed class MessagingInfrastructureRegistrationMarker;
}
