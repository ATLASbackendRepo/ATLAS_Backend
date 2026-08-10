namespace ATLAsBLL.Behaviors.Caching
{
    public interface ICachableQuery
    {
        string CacheKey { get; }
        System.TimeSpan? Expiration { get; }
    }
}