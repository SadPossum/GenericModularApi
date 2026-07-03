namespace Shared.Messaging.Nats.Aspire;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shared.Infrastructure;
using Shared.Infrastructure.Messaging;

public static class DependencyInjection
{
    private const string ConnectionName = "nats";

    public static IHostApplicationBuilder AddConfiguredNatsJetStreamMessaging(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        IConfigurationSection section = builder.Configuration.GetSection(NatsJetStreamOptions.SectionName);
        NatsJetStreamOptions natsOptions = section.Get<NatsJetStreamOptions>() ?? new NatsJetStreamOptions();

        if (!natsOptions.Enabled)
        {
            return builder;
        }

        ValidateNatsOptions(natsOptions);
        string? connectionString = builder.Configuration.GetConnectionString(ConnectionName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new OptionsValidationException(
                NatsJetStreamOptions.SectionName,
                typeof(NatsJetStreamOptions),
                [$"ConnectionStrings:{ConnectionName} is required when NATS JetStream publishing is enabled."]);
        }

        builder.AddNatsClient(ConnectionName, (_, options) =>
        {
            return options with { Url = connectionString };
        });
        builder.AddNatsJetStreamMessaging();

        return builder;
    }

    private static void ValidateNatsOptions(NatsJetStreamOptions options)
    {
        ValidateOptionsResult result = new NatsJetStreamOptionsValidator().Validate(name: null, options);

        if (result.Failed)
        {
            throw new OptionsValidationException(NatsJetStreamOptions.SectionName, typeof(NatsJetStreamOptions), result.Failures);
        }
    }
}
