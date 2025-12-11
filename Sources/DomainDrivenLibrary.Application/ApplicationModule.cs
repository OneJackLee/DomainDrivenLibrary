using System.Reflection;
using DomainDrivenLibrary.Dependencies;
using Microsoft.Extensions.DependencyInjection;

namespace DomainDrivenLibrary;

public static class ApplicationModule
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var types = Assembly.GetExecutingAssembly().GetTypes()
            .Where(x => x is { IsClass: true, IsAbstract: false })
            .ToList();

        return services.AddDependencies(types);
    }
}