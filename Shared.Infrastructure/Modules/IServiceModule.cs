namespace Shared.Infrastructure.Modules;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public interface IServiceModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration);
}
