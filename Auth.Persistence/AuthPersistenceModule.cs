namespace Auth.Persistence;

using Auth.Application;
using Auth.Domain.Repositories;
using Auth.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Modules;

public class AuthInfrastructureModule : IServiceModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration) =>
        services.AddDbContext<AuthDbContext>(options =>
            options.UseSqlServer(configuration
                .GetConnectionString("DefaultConnection")))
        .AddScoped<IAuthUnitOfWork, AuthUnitOfWork>()
        .AddScoped<IMemberRepository, MemberRepository>();
}
