using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Users.Commands.CreateUsersRange
{
    public class CreateUsersRangeCommandHandler : IRequestHandler<CreateUsersRangeCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateUsersRangeCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> Handle(CreateUsersRangeCommand request, CancellationToken cancellationToken)
        {
            // 1. تحويل الـ DTOs القادمة في الطلب إلى كائنات Entity
            var usersEntities = new List<User>();

            foreach (var u in request.Users)
            {
                var user = new User
                {
                    FullName = u.FullName,
                    Role = u.Role
                };

                user.Password = BCrypt.Net.BCrypt.HashPassword(u.Password);
                usersEntities.Add(user);
            }

            // 2. استخدام دالة الـ AddRange المتاحة في الـ Repository عبر الـ Instance
            await _unitOfWork.Users.AddRangeAsync(usersEntities);

            // 3. حفظ التغييرات كاملة كـ Transaction واحدة في قاعدة البيانات
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 4. إرجاع قائمة تحتوي على الـ IDs الجديدة التي تم إنشاؤها تلقائياً
            return "inserted successfully";
        }
    }
}