using MediatR;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Behaviors
{
    public class RateLimitingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        // لتخزين أوقات آخر الريكويستات في الذاكرة مؤقتاً
        private static readonly ConcurrentDictionary<string, DateTime> _requestTimes = new();

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // بنطبق الرقابة على الـ Commands الحساسة فقط كمثال
            if (typeof(TRequest).Name.StartsWith("Create") || typeof(TRequest).Name.StartsWith("Update"))
            {
                var key = typeof(TRequest).Name; // في الحقيقي بنربطه بالـ User ID أو الـ IP
                var now = DateTime.UtcNow;

                if (_requestTimes.TryGetValue(key, out var lastRequestTime))
                {
                    // منع تنفيذ الطلب لو كان الفرق بين الريكويست الحالي والسابق أقل من ثانية واحدة
                    if (now - lastRequestTime < TimeSpan.FromSeconds(1))
                    {
                        throw new InvalidOperationException("Too many requests. Please wait a moment before trying again.");
                    }
                }

                _requestTimes[key] = now;
            }

            return await next();
        }
    }
}