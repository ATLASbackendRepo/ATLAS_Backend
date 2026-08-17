using ATLAsBLL.Events;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Features.Students.Commands.CreateStudentsRange
{
    public class CreateStudentsRangeCommandHandler : IRequestHandler<CreateStudentsRangeCommand, List<int>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher; 

        public CreateStudentsRangeCommandHandler(IUnitOfWork unitOfWork, IPublisher publisher       )
        {
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<List<int>> Handle(CreateStudentsRangeCommand request, CancellationToken cancellationToken)
        {
            var studentsList = new List<Student>();

            foreach (var item in request.Students)
            {
                // 1. إنشاء كائن المستخدم الخاص بكل طالب في الذاكرة
                var user = new User
                {
                    FullName = item.FullName
                };

                user.Password = BCrypt.Net.BCrypt.HashPassword(item.Password);

                // 2. إنشاء كائن الطالب وربطه بالمستخدم المجهز له
                var student = new Student
                {
                    StudentCode = item.StudentCode,
                    AcademicId = item.AcademicId,
                    AcademicLevel = item.AcademicLevel,
                    DepartmentId = item.DepartmentId,
                    User = user // الـ EF Core سيقوم بإنشاء الـ User تلقائياً ويأخذ الـ Id الخاص به للـ Student
                };

                studentsList.Add(student);
            }

            // 3. إضافة القائمة بالكامل إلى الـ Repository في خطوة واحدة
            await _unitOfWork.Students.AddRangeAsync(studentsList);

            // 4. حفظ كل الطلاب مع كل المستخدمين المرتبطين بهم كـ Transaction واحدة آمنة
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var Student in studentsList)
            {
                await _publisher.Publish(
                    new StudentCreatedDomainEvent(Student.StudentId),
                    cancellationToken
                );
            }


            // 5. إرجاع قائمة الـ StudentIds الجديدة التي تم توليدها
            return studentsList.Select(s => s.StudentId).ToList();
        }
    }
}
