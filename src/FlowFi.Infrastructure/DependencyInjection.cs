using FlowFi.Application.Common.Interfaces;
using FlowFi.Infrastructure.Persistence;
using FlowFi.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowFi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Database"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
                      .EnableRetryOnFailure(maxRetryCount: 3)
            )
        );
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["Redis:ConnectionString"];
            options.InstanceName  = "flowfi:";
        });

        services.AddScoped<ITokenService,    TokenService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ICacheService,    CacheService>();
        services.AddScoped<IAiService,       AiService>();

        return services;
    }
}
