using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;
using ATLAsBLL.Events;

namespace ATLAsBLL.Features.Students.Commands.CreateStudent
{
    public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;

        public CreateStudentCommandHandler(IUnitOfWork unitOfWork, IPublisher publisher)
        {
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<int> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
        {
            if (await _unitOfWork.Students.StudentCodeExistsAsync(request.StudentCode))
                throw new InvalidOperationException("Student code already exists.");

            var user = new User
            {
                FullName = request.FullName
                ,
                AcademicId = request.AcademicId 
            };

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // 2. إنشاء كائن الـ Studentوربطه بكائن الـ User مباشرة
            var student = new Student
            {
                StudentCode = request.StudentCode,
                AcademicId = request.AcademicId,
                AcademicLevel = request.AcademicLevel,
                DepartmentId = request.DepartmentId,
                User = user // الربط المباشر هنا بيخلي الـ EF يتعامل مع الـ IDs تلقائياً
            };

            // 3. إضافة الطالب (الـ EF هيفهم تلقائياً إنه لازم يضيف الـ User المرتبط بيه كمان)
            await _unitOfWork.Students.AddAsync(student);

            // 4. حفظ الاتنين معاً كـ Transaction واحدة
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 5. إعلام بأن الطالب تم إنشاؤه
            await _publisher.Publish(new StudentCreatedDomainEvent(student.StudentId), cancellationToken);

            return student.StudentId;
        }
    }
}
