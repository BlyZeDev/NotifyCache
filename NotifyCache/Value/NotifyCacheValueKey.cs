namespace NotifyCache.Value;

public readonly record struct NotifyCacheValueKey<T> : INotifyCacheKey<T> where T : struct
{
    public T Key { get; }

    public NotifyCacheValueKey(T key) => Key = key;

    public static implicit operator NotifyCacheValueKey<T>(T key) => new NotifyCacheValueKey<T>(key);
}