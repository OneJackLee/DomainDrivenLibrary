using Microsoft.Extensions.DependencyInjection;

namespace DomainDrivenLibrary.Dependencies;

public static class DependencyRegistrator
{
    public static IServiceCollection AddDependencies(this IServiceCollection services, List<Type> types)
    {
        foreach (Type type in types)
        {
            if (type.IsAssignableTo(typeof(ITransientDependency)))
                RegisterTransient(services, type);
            else if (type.IsAssignableTo(typeof(IScopedDependency)))
                RegisterScoped(services, type);
            else if (type.IsAssignableTo(typeof(ISingletonDependency)))
                RegisterSingleton(services, type);
        }

        return services;
    }

    private static void RegisterSingleton(IServiceCollection services, Type type)
    {
        services.AddSingleton(type);

        Type[] interfaces = type.GetInterfaces();
        foreach (Type interfaceType in interfaces)
        {
            string interfaceName = interfaceType.Name[1..];
            if (type.Name.EndsWith(interfaceName, StringComparison.InvariantCultureIgnoreCase))
                services.AddSingleton(interfaceType, type);
        }
    }

    private static void RegisterScoped(IServiceCollection services, Type type)
    {
        services.AddScoped(type);

        Type[] interfaces = type.GetInterfaces();
        foreach (Type interfaceType in interfaces)
        {
            string interfaceName = interfaceType.Name[1..];
            if (type.Name.EndsWith(interfaceName, StringComparison.InvariantCultureIgnoreCase))
                services.AddScoped(interfaceType, type);
        }
    }

    private static void RegisterTransient(IServiceCollection services, Type type)
    {
        services.AddTransient(type);

        Type[] interfaces = type.GetInterfaces();
        foreach (Type interfaceType in interfaces)
        {
            string interfaceName = interfaceType.Name[1..];
            if (type.Name.EndsWith(interfaceName, StringComparison.InvariantCultureIgnoreCase))
                services.AddTransient(interfaceType, type);
        }
    }
}