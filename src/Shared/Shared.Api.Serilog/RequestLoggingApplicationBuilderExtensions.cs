namespace Shared.Api.Serilog;

using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using global::Serilog;
using Shared.Api.Observability;
using Shared.Application.Observability;
using Shared.Application.Tenancy;

public static class RequestLoggingApplicationBuilderExtensions
{
    public static IApplicationBuilder UseGmaSerilogRequestLogging(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                ModuleEndpointMetadata? module = httpContext.GetEndpoint()?.Metadata.GetMetadata<ModuleEndpointMetadata>();
                ITenantContext? tenantContext = httpContext.RequestServices.GetService<ITenantContext>();
                string traceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;

                diagnosticContext.Set(ObservabilityLogPropertyNames.TraceId, traceId);

                if (module is not null)
                {
                    diagnosticContext.Set(ObservabilityLogPropertyNames.Module, module.ModuleName);
                }

                if (!string.IsNullOrWhiteSpace(tenantContext?.TenantId))
                {
                    diagnosticContext.Set(ObservabilityLogPropertyNames.TenantId, tenantContext.TenantId);
                }
            };
        });
    }
}
