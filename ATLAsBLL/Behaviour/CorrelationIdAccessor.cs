using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ATLAsBLL.Behaviour
{
    /// <summary>
    /// ICorrelation Id Accessor.
    /// </summary>
    public interface ICorrelationIdAccessor
    {
        string? CorrelationId { get; }
    }
    /// <summary>
    /// Correlation Id Accessor.
    /// </summary>
    public class CorrelationIdAccessor : ICorrelationIdAccessor
    {
        /// <summary>
        /// _http Context Accessor.
        /// </summary>
        private readonly IHttpContextAccessor  _httpContextAccessor;

        public CorrelationIdAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Correlation Id.
        /// </summary>
        public string? CorrelationId =>
            // لو عندك Middleware بيحط CorrelationId في الـ Response Headers أو الـ Items، هنجيبه من هناك
            _httpContextAccessor.HttpContext?.TraceIdentifier
            ?? Activity.Current?.Id;
    }
}
