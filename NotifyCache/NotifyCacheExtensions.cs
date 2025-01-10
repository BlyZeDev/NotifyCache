namespace NotifyCache;

using Microsoft.Extensions.DependencyInjection;

public static class NotifyCacheExtensions
{
    public static IServiceCollection AddNotifyCache(this IServiceCollection services)
        => services.AddNotifyCache<NotifyCache>();

    public static IServiceCollection AddNotifyCache<T>(this IServiceCollection services) where T : class, INotifyCache
        => services.AddNotifyCache(x => new NotifyCache());

    public static IServiceCollection AddNotifyCache<T>(this IServiceCollection services, Func<IServiceProvider, T> implementationFactory) where T : class, INotifyCache
        => services.AddSingleton<INotifyCache, T>(implementationFactory);
}