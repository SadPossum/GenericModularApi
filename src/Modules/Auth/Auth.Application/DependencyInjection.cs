namespace Auth.Application;

using Auth.Application.Commands;
using Auth.Application.Handlers;
using Auth.Application.Queries;
using Auth.Application.Validation;
using Auth.Contracts;
using Auth.Domain.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shared.Application;
using Shared.Application.Cqrs;
using Shared.Application.Events;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthApplication(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        if (!services.Any(descriptor => descriptor.ServiceType == typeof(AuthApplicationOptionsRegistrationMarker)))
        {
            AuthApplicationOptionsValidation.GetValidatedOptions(configuration);
            services.AddSingleton<AuthApplicationOptionsRegistrationMarker>();
            services
                .AddOptions<AuthApplicationOptions>()
                .Bind(configuration.GetSection(AuthApplicationOptions.SectionName))
                .ValidateOnStart();
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IValidateOptions<AuthApplicationOptions>, AuthApplicationOptionsValidator>());
        }

        services.TryAddEnumerable([
            ServiceDescriptor.Scoped<ICommandHandler<RegisterMemberCommand, AuthTokensResponse>, RegisterMemberCommandHandler>(),
            ServiceDescriptor.Scoped<ICommandHandler<LoginMemberCommand, AuthTokensResponse>, LoginMemberCommandHandler>(),
            ServiceDescriptor.Scoped<ICommandHandler<RefreshMemberSessionCommand, AuthTokensResponse>, RefreshMemberSessionCommandHandler>(),
            ServiceDescriptor.Scoped<ICommandHandler<SignOutCommand, Unit>, SignOutCommandHandler>(),
            ServiceDescriptor.Scoped<ICommandHandler<SignOutAllCommand, Unit>, SignOutAllCommandHandler>(),
            ServiceDescriptor.Scoped<ICommandHandler<AdminCreateMemberCommand, AdminCreatedMemberResponse>, AdminCreateMemberCommandHandler>(),
            ServiceDescriptor.Scoped<ICommandHandler<DisableMemberCommand, Unit>, DisableMemberCommandHandler>(),
            ServiceDescriptor.Scoped<ICommandHandler<EnableMemberCommand, Unit>, EnableMemberCommandHandler>(),
            ServiceDescriptor.Scoped<ICommandHandler<ResetMemberPasswordCommand, Unit>, ResetMemberPasswordCommandHandler>(),
            ServiceDescriptor.Scoped<ICommandHandler<RevokeMemberSessionsCommand, AdminRevokeSessionsResponse>, RevokeMemberSessionsCommandHandler>(),
            ServiceDescriptor.Scoped<IQueryHandler<ListAdminMembersQuery, AdminMemberListResponse>, ListAdminMembersQueryHandler>(),
            ServiceDescriptor.Scoped<IQueryHandler<GetAdminMemberQuery, AdminMemberDetails>, GetAdminMemberQueryHandler>(),
            ServiceDescriptor.Scoped<IDomainEventHandler<MemberRegisteredDomainEvent>, MemberRegisteredOutboxProjector>(),
            ServiceDescriptor.Scoped<IDomainEventHandler<MemberDisabledDomainEvent>, MemberDisabledOutboxProjector>(),
            ServiceDescriptor.Scoped<IDomainEventHandler<MemberEnabledDomainEvent>, MemberEnabledOutboxProjector>(),
            ServiceDescriptor.Scoped<IDomainEventHandler<MemberSessionsRevokedDomainEvent>, MemberSessionsRevokedOutboxProjector>(),
            ServiceDescriptor.Scoped<ICommandValidator<RegisterMemberCommand>, RegisterMemberCommandValidator>(),
            ServiceDescriptor.Scoped<ICommandValidator<LoginMemberCommand>, LoginMemberCommandValidator>(),
            ServiceDescriptor.Scoped<ICommandValidator<RefreshMemberSessionCommand>, RefreshMemberSessionCommandValidator>(),
            ServiceDescriptor.Scoped<ICommandValidator<SignOutCommand>, SignOutCommandValidator>(),
            ServiceDescriptor.Scoped<ICommandValidator<SignOutAllCommand>, SignOutAllCommandValidator>(),
            ServiceDescriptor.Scoped<ICommandValidator<AdminCreateMemberCommand>, AdminCreateMemberCommandValidator>(),
            ServiceDescriptor.Scoped<ICommandValidator<DisableMemberCommand>, DisableMemberCommandValidator>(),
            ServiceDescriptor.Scoped<ICommandValidator<EnableMemberCommand>, EnableMemberCommandValidator>(),
            ServiceDescriptor.Scoped<ICommandValidator<ResetMemberPasswordCommand>, ResetMemberPasswordCommandValidator>(),
            ServiceDescriptor.Scoped<ICommandValidator<RevokeMemberSessionsCommand>, RevokeMemberSessionsCommandValidator>(),
            ServiceDescriptor.Scoped<IQueryValidator<GetAdminMemberQuery>, GetAdminMemberQueryValidator>()
        ]);

        return services;
    }

    private sealed class AuthApplicationOptionsRegistrationMarker;
}
