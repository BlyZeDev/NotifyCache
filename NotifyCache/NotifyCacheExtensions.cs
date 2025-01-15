namespace NotifyCache;

using Microsoft.Extensions.DependencyInjection;

public static class NotifyCacheExtensions
{
    public static IServiceCollection AddNotifyCache<TKey>(this IServiceCollection services)
        where TKey : notnull
        => services.AddNotifyCache<NotifyCache<TKey>>();

    public static IServiceCollection AddNotifyCache<TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory)
        where TImplementation : class
        => services.AddSingleton(implementationFactory);
}