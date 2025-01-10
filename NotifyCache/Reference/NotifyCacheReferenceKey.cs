namespace NotifyCache.Reference;

public sealed record NotifyCacheReferenceKey<T> : INotifyCacheKey<T> where T : class
{
    public T Key { get; }

    public NotifyCacheReferenceKey(T key) => Key = key;

    public static implicit operator NotifyCacheReferenceKey<T>(T key) => new NotifyCacheReferenceKey<T>(key);
}