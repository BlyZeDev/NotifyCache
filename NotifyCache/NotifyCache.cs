namespace NotifyCache;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public sealed class NotifyCache<TKey> where TKey : notnull
{
    private readonly Lock _lock;
    private readonly Dictionary<TKey, INotifyCacheItem> _cache;
    private readonly OrderedDictionary<long, TKey> _expirationUnixSeconds;

    public event OnExpiration<TKey>? ItemExpired;

    public NotifyCache(IEqualityComparer<TKey>? equalityComparer = null)
    {
        _lock = new Lock();
        _cache = new Dictionary<TKey, INotifyCacheItem>(equalityComparer);
        _expirationUnixSeconds = [];
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

    public bool TryAdd<TValue>(TKey key, TValue item, DateTimeOffset expiration) where TValue : notnull
    {
        if (expiration <= DateTimeOffset.Now) throw new ArgumentException($"{nameof(expiration)} has to be in the future", nameof(expiration));

        lock (_lock)
        {
            var unixSeconds = expiration.ToUnixTimeSeconds();

            var success = _cache.TryAdd(key, new InternalNotifyCacheItem<TValue>
            {
                Value = item,
                UnixSeconds = unixSeconds
            });

            if (success) _expirationUnixSeconds.Add(unixSeconds, key);

            return success;
        }
    }

    public bool TryRemove(TKey key)
    {
        lock (_lock)
        {
            var success = _cache.Remove(key, out var item);

            if (success) _expirationUnixSeconds.Remove(item!.UnixSeconds);

            return success;
        }
    }

    private async Task CheckExpirationsAsync()
    {
        using (var timer = new PeriodicTimer(TimeSpan.FromSeconds(1)))
        {
            while (await timer.WaitForNextTickAsync())
            {
                lock (_lock)
                {
                    var now = DateTimeOffset.Now.ToUnixTimeSeconds();
                    var enumerator = _expirationUnixSeconds.Keys.GetEnumerator();

                    for (int i = 0; enumerator.MoveNext(); i++)
                    {
                        if (enumerator.Current >= now)
                        {
                            if (_expirationUnixSeconds.Remove(enumerator.Current))
                                ItemExpired?.Invoke(_expirationUnixSeconds.GetAt(i).Value);
                        }
                    }
                }
            }
        }
    }
}

public delegate void OnExpiration<TKey>(TKey key);