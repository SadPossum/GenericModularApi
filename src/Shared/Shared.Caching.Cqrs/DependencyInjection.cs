namespace Shared.Caching.Cqrs;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shared.Caching;
using Shared.Caching.Infrastructure;
using Shared.Cqrs;
using Shared.Cqrs.Infrastructure;
using Shared.ModuleComposition;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddCachingCqrs(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddCachingInfrastructure();
        builder.AddCqrsInfrastructure();

        if (builder.Services.Any(descriptor => descriptor.ServiceType == typeof(CachingCqrsRegistrationMarker)))
        {
            return builder;
        }

        builder.Services.AddSingleton<CachingCqrsRegistrationMarker>();
        builder.ProvideFeature(CachingCompositionFeatures.CqrsInvalidationProvided("Shared.Caching.Cqrs"));
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Scoped(typeof(ICommandPipelineBehavior<,>), typeof(CacheInvalidationCommandBehavior<,>)));
        builder.Services.MoveCommandUnitOfWorkBehaviorToEnd();

        return builder;
    }

    private sealed class CachingCqrsRegistrationMarker;
}
