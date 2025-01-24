namespace NotifyCacheTests;

using NotifyCache;

sealed class Program
{
    static async Task Main()
    {
        var start = DateTimeOffset.UtcNow;

        _ = new Timer((state) => Console.WriteLine((DateTimeOffset.UtcNow - start).Seconds), null, 1000, 1000);

        var cache = new NotifyCache<int>();

        cache.ItemExpired += (key, item) => Console.WriteLine($"Key: {key}\nItem: {item}");

        cache.TryAdd(69, "Test", TimeSpan.FromSeconds(10));
        cache.TryAdd(420, 6.9, TimeSpan.FromSeconds(15));
        cache.TryAdd(300, DateTime.UtcNow, TimeSpan.FromSeconds(2));

        cache.TryGet<DateTime>(300, out var value3);
        cache.TryGet<double>(420, out var value1);
        cache.TryGet<string>(69, out var value2);

        Console.WriteLine(value3);
        Console.WriteLine(value1);
        Console.WriteLine(value2);

        await Task.Delay(Timeout.Infinite);
    }
}