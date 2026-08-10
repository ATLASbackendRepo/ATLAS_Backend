using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork; // تأكد من وجود آلية الـ Transaction في الـ Unit of Work عندك

namespace ATLAsBLL.Behaviors
{
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

        public TransactionBehavior(IUnitOfWork unitOfWork, ILogger<TransactionBehavior<TRequest, TResponse>> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
         
            if (request is IQuery<TResponse>)
                return await next();

            if (request is not ICommand<TResponse>)
                return await next();

   
            var requestName = typeof(TRequest).Name;
            _logger.LogInformation("--> Opening transaction for {RequestName}", requestName);

            try
            {
                // بدء الـ Transaction من الـ Unit of Work أو الـ DbContext مباشرة
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                var response = await next();

                // لو الـ Handler خلص بدون أي Exceptions، بنعمل Commit أوتوماتيك
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                _logger.LogInformation("--> Transaction committed successfully for {RequestName}", requestName);

                return response;
            }
            catch (Exception ex)
            {
                // في حالة حدوث أي خطأ، بنلغي كل العمليات اللي تمت ونرجع الداتابيز لحالتها السابقة فوراً
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "X--> Transaction rolled back due to an error in {RequestName}", requestName);
                throw;
            }
        }
    }
}
