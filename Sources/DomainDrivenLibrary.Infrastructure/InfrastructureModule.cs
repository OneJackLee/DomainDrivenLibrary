using System.Reflection;
using DomainDrivenLibrary.Data;
using DomainDrivenLibrary.Dependencies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace DomainDrivenLibrary;

public static class InfrastructureModule
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var types = Assembly.GetExecutingAssembly().GetTypes()
            .Where(x => x is { IsClass: true, IsAbstract: false })
            .ToList();

        return services.AddDependencies(types)
            .AddPersistence(configuration);
    }

    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default") ??
            throw new ArgumentNullException(nameof(configuration));

        var dataSource = new NpgsqlDataSourceBuilder(connectionString)
            .EnableDynamicJson()
            .Build();

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(dataSource).UseSnakeCaseNamingConvention());

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }
}