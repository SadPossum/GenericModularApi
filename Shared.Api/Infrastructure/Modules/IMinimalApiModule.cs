namespace Shared.Api.Infrastructure.Modules;

using Microsoft.AspNetCore.Routing;

public interface IMinimalApiModule
{
    public void AddRoutes(IEndpointRouteBuilder endpointRouteBuilder);
}
