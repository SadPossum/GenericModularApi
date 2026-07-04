namespace Administration.Persistence;

using Administration.Application.Ports;
using Administration.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shared.Administration;
using Shared.Cqrs.UnitOfWork;
using Shared.Persistence.EntityFrameworkCore;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddAdministrationPersistence(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddPersistenceOptions(builder.Configuration);

        builder.Services.TryAddModuleDbContext<AdminDbContext>(options =>
            options.UseConfiguredProvider(
                builder.Configuration,
                AdminMigrations.SqlServerAssembly,
                AdminMigrations.PostgreSqlAssembly,
                AdminMigrations.Schema,
                AdminMigrations.HistoryTable));

        builder.Services.TryAddScoped<IAdminRbacRepository, AdminRbacRepository>();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped<IUnitOfWork, AdminUnitOfWork>());
        builder.Services.Replace(ServiceDescriptor.Scoped<IAdminAuditSink, AdminAuditSink>());

        return builder;
    }
}
