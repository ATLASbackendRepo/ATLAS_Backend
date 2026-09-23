namespace ATLAsBLL.Behaviors.Caching
{
    /// <summary>
    /// ICachable Query.
    /// </summary>
    public interface ICachableQuery
    {
        string CacheKey { get; }
        System.TimeSpan? Expiration { get; }
    }
}
