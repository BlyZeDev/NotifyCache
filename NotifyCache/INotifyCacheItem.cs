namespace NotifyCache;

internal interface INotifyCacheItem
{
    public long UnixSeconds { get; }

    public object ValueObj { get; }
}