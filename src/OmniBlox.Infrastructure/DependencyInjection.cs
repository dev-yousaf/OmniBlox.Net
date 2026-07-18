using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Infrastructure.Auth;
using OmniBlox.Infrastructure.Persistence;
using OmniBlox.Infrastructure.Services;

namespace OmniBlox.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<AppDbContext>());

        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.Configure<JwtSettings>(
            configuration.GetSection("Jwt"));

        services.AddScoped<IJwtService, JwtService>();

        services.AddScoped<IStockService, StockService>();

        return services;
    }
}
