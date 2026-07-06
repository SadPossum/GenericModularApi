using Administration.AdminCli;
using Auth.AdminCli;
using Auth.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shared.Administration.Cli;
using Shared.Caching.Cqrs;
using Shared.Caching.Redis;
using Shared.Infrastructure;
using Shared.Messaging.Infrastructure;
using Shared.ModuleComposition;
using Shared.Tenancy.Caching;
using Shared.Tenancy.Messaging.Infrastructure;
using System.CommandLine;
using System.CommandLine.Parsing;

try
{
    HostApplicationBuilder builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(
        new HostApplicationBuilderSettings
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory
        });

    builder.Services.AddSharedAdministrationCli();
    builder.AddRedisCaching();
    builder.AddCachingCqrs();
    builder.AddSharedInfrastructure();
    builder.AddTenantCaching();
    builder.AddMessagingInfrastructure();
    builder.AddTenantAwareMessaging();
    builder.AddAdminModule<AdministrationAdminCliModule>();
    builder.AddAuthAdminModule(AuthProfile.TenantScoped());
    builder.ValidateModuleComposition();

    using IHost host = builder.Build();

    host.Services.ValidateAdminCliStartup();
    RootCommand rootCommand = host.Services.CreateAdminRootCommand();
    ParseResult parseResult = rootCommand.Parse(args);
    InvocationConfiguration invocation = new()
    {
        EnableDefaultExceptionHandler = false
    };

    return await parseResult.InvokeAsync(invocation, CancellationToken.None).ConfigureAwait(false);
}
catch (OperationCanceledException)
{
    AdminCliOutput.WriteError("Admin command was canceled.");
    return AdminExitCodes.Failed;
}
catch (OptionsValidationException exception)
{
    AdminCliOutput.WriteError("Admin CLI configuration is invalid.");

    foreach (string failure in exception.Failures.Distinct(StringComparer.Ordinal))
    {
        AdminCliOutput.WriteError(failure);
    }

    return AdminExitCodes.Failed;
}
catch (ModuleCompositionValidationException exception)
{
    AdminCliOutput.WriteError("Admin CLI module composition is invalid.");

    foreach (string error in exception.Errors)
    {
        AdminCliOutput.WriteError(error);
    }

    return AdminExitCodes.Failed;
}
catch (Exception)
{
    AdminCliOutput.WriteError("Admin command failed unexpectedly.");
    return AdminExitCodes.Failed;
}
