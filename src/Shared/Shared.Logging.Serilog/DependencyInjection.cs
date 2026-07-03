namespace Shared.Logging.Serilog;

using Microsoft.Extensions.Hosting;
using global::Serilog;

public static class DependencyInjection
{
    public static IHostBuilder UseConfiguredSerilog(this IHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.UseSerilog((context, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration));
    }
}
