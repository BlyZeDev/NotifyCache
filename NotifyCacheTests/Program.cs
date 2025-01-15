namespace NotifyCacheTests;

using NotifyCache;

sealed class Program
{
    static void Main()
    {
        var cache = new NotifyCache<int>();

        cache.TryAdd(69, "Test");
        cache.TryAdd(420, 6.9);

        cache.TryGet<double>(420, out var value1);
        cache.TryGet<string>(69, out var value2);

        Console.WriteLine(value1);
        Console.WriteLine(value2);
    }
}