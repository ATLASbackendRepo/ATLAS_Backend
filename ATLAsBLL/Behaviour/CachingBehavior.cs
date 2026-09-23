using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Behaviors.Caching
{
    /// <summary>
    /// Caching Behavior.
    /// </summary>
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// _cache.
        /// </summary>
        private readonly IMemoryCache _cache;
        /// <summary>
        /// _logger.
        /// </summary>
        private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

        public CachingBehavior(IMemoryCache cache, ILogger<CachingBehavior<TRequest, TResponse>> logger)
        {
            _cache = cache;
            _logger = logger;
            }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // لو الـ Request مش مـورث من واجهة الكاش، كمل طريقك للـ Handler العادي
            if (request is not ICachableQuery cachableQuery)
            {
                return await next();
            }

            var cacheKey = cachableQuery.CacheKey;


            if (_cache.TryGetValue(cacheKey, out object? cachedValue) &&
                cachedValue is TResponse cachedResponse)
            {
                _logger.LogInformation(
                    "--> [CACHE HIT] Returning cached data for Key: {CacheKey}",
                    cacheKey);

                return cachedResponse;
            }

            _logger.LogInformation("--> [CACHE MISS] Fetching data from DB for Key: {CacheKey}", cacheKey);

            // جلب البيانات الفعلي من قاعدة البيانات عبر الـ Handler
            var response = await next();

            // إعداد وقت انتهاء صلاحية الكاش (مثلاً ساعة لو مش محدد)
            var expirationTime = cachableQuery.Expiration ?? TimeSpan.FromHours(1);

            _cache.Set(cacheKey, response, expirationTime);

            return response;
        }
    }
}
