namespace NotifyCache;

internal sealed class InternalNotifyCacheItem<T> : INotifyCacheItem where T : notnull
{
    public required T Value { get; init; }

    public required long UnixSeconds { get; init; }
}