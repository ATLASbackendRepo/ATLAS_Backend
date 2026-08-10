using FluentValidation;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        // MediatR هيحقن هنا تلقائياً أي Validator متوافق مع الـ Command اللي شغال
        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                // تشغيل الـ Validation على الـ Request بشكل غير متزامن
                var validationResults = await Task.WhenAll(
                    _validators.Select(v => v.ValidateAsync(context, cancellationToken))
                );

                // تجميع كل الأخطاء (Failures) المكتشفة
                var failures = validationResults
                    .SelectMany(r => r.Errors)
                    .Where(f => f != null)
                    .ToList();

                // لو لقَّط أي غلطة، يفرمل الـ Request ويرمي Exception مخصص
                if (failures.Count != 0)
                {
                    throw new ValidationException(failures);
                }
            }

            // لو الداتا سليمة تماماً، يعديه للخطوة اللي بعدها في الـ Pipeline أو للـ Handler
            return await next();
        }
    }
}