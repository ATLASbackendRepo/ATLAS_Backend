using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ATLAsBLL.Behaviour
{
    public interface ICorrelationIdAccessor
    {
        string? CorrelationId { get; }
    }
    public class CorrelationIdAccessor : ICorrelationIdAccessor
    {
        private readonly IHttpContextAccessor  _httpContextAccessor;

        public CorrelationIdAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? CorrelationId =>
            // لو عندك Middleware بيحط CorrelationId في الـ Response Headers أو الـ Items، هنجيبه من هناك
            _httpContextAccessor.HttpContext?.TraceIdentifier
            ?? Activity.Current?.Id;
    }
}
