namespace Administration.Application;

using Administration.Application.Commands;
using Administration.Application.Handlers;
using Administration.Application.Queries;
using Administration.Application.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shared.Administration;
using Shared.Application;
using Shared.Application.Cqrs;

public static class DependencyInjection
{
    public static IServiceCollection AddAdministrationApplication(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        if (!services.Any(descriptor => descriptor.ServiceType == typeof(AdministrationOptionsRegistrationMarker)))
        {
            AdministrationOptionsValidation.GetValidatedOptions(configuration);
            services.AddSingleton<AdministrationOptionsRegistrationMarker>();
            services
                .AddOptions<AdministrationOptions>()
                .Bind(configuration.GetSection(AdministrationOptions.SectionName))
                .ValidateOnStart();
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IValidateOptions<AdministrationOptions>, AdministrationOptionsValidator>());
        }

        services.Replace(ServiceDescriptor.Scoped<IAdminAuthorizationService, PersistedAdminAuthorizationService>());
        services.TryAddEnumerable([
            ServiceDescriptor.Scoped<ICommandHandler<BootstrapOwnerCommand, Unit>, BootstrapOwnerCommandHandler>(),
            ServiceDescriptor.Scoped<ICommandHandler<CreateRoleCommand, AdminRoleDetails>, CreateRoleCommandHandler>(),
            ServiceDescriptor.Scoped<ICommandHandler<GrantRolePermissionCommand, Unit>, GrantRolePermissionCommandHandler>(),
            ServiceDescriptor.Scoped<ICommandHandler<AssignRoleCommand, Unit>, AssignRoleCommandHandler>(),
            ServiceDescriptor.Scoped<IQueryHandler<ListRolesQuery, IReadOnlyList<AdminRoleDetails>>, ListRolesQueryHandler>(),
            ServiceDescriptor.Scoped<ICommandValidator<BootstrapOwnerCommand>, BootstrapOwnerCommandValidator>(),
            ServiceDescriptor.Scoped<ICommandValidator<CreateRoleCommand>, CreateRoleCommandValidator>(),
            ServiceDescriptor.Scoped<ICommandValidator<GrantRolePermissionCommand>, GrantRolePermissionCommandValidator>(),
            ServiceDescriptor.Scoped<ICommandValidator<AssignRoleCommand>, AssignRoleCommandValidator>()
        ]);

        return services;
    }

    private sealed class AdministrationOptionsRegistrationMarker;
}
