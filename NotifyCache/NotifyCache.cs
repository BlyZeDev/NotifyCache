namespace NotifyCache;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public sealed class NotifyCache<TKey> : IDisposable where TKey : notnull
{
    private readonly Lock _lock;
    private readonly Dictionary<TKey, INotifyCacheItem> _cache;
    private readonly OrderedDictionary<long, TKey> _expirationUnixMs;
    private Task? nextExpirationTask;

    public event OnExpiration<TKey>? ItemExpired;

    public NotifyCache(IEqualityComparer<TKey>? equalityComparer = null)
    {
        _lock = new Lock();
        _cache = new Dictionary<TKey, INotifyCacheItem>(equalityComparer);
        _expirationUnixMs = [];
    }

    public bool TryGet<TValue>(TKey key, out TValue value) where TValue : notnull
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(key, out var item) && item is InternalNotifyCacheItem<TValue> result)
            {
                value = result.Value;
                return true;
            }

            value = default!;
            return false;
        }
    }

    public bool TryAdd<TValue>(TKey key, TValue item) where TValue : notnull
        => TryAdd(key, item, long.MinValue);

    public bool TryAdd<TValue>(TKey key, TValue item, in TimeSpan expireIn) where TValue : notnull
        => TryAdd(key, item, DateTimeOffset.UtcNow.Add(expireIn).ToUnixTimeMilliseconds());

    public bool TryRemove(TKey key)
    {
        lock (_lock)
        {
            var success = _cache.Remove(key, out var item);

            if (success) _expirationUnixMs.Remove(item!.UnixSeconds);

            return success;
        }
    }

    public void Dispose() => nextExpirationTask?.Dispose();
    
    private bool TryAdd<TValue>(TKey key, TValue item, long expirationUnixMs) where TValue : notnull
    {
        if (expirationUnixMs > long.MinValue)
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(expirationUnixMs, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), nameof(expirationUnixMs));
        
        lock (_lock)
        {
            var success = _cache.TryAdd(key, new InternalNotifyCacheItem<TValue>
            {
                Value = item,
                UnixSeconds = expirationUnixMs
            });

            if (success && expirationUnixMs > long.MinValue)
            {
                _expirationUnixMs.Add(expirationUnixMs, key);

                nextExpirationTask = null;
                nextExpirationTask = Task.Run(async () => await FireNextExpirationAsync(expirationUnixMs));
            }

            return success;
        }
    }

    public async Task FireNextExpirationAsync(long atUnixMs)
    {
        await Task.Delay(DateTimeOffset.FromUnixTimeMilliseconds(atUnixMs).Subtract(DateTimeOffset.UtcNow));

        if (_expirationUnixMs.Remove(atUnixMs, out var key))
            ItemExpired?.Invoke(key);

        atUnixMs = _expirationUnixMs.GetAt(0).Key;
        nextExpirationTask = Task.Run(async () => await FireNextExpirationAsync(atUnixMs));
    }
}

public delegate void OnExpiration<TKey>(TKey key);