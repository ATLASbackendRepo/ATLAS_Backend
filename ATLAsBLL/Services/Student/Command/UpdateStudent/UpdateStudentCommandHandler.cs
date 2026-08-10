using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Students.Commands.UpdateStudent
{
    public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateStudentCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
        {
            // جلب الطالب مع الـ User المرتبط به من قاعدة البيانات
            var student = await _unitOfWork.Students.GetStudentWithUserAsync(request.StudentId);

            if (student == null)
            {
                throw new System.Collections.Generic.KeyNotFoundException($"Student with ID {request.StudentId} was not found.");
            }

            // 1. تحديث بيانات جدول الـ User الأساسية
            student.User.FullName = request.FullName;
     

            // 2. تحديث بيانات جدول الـ Student الخاصة به
            student.StudentCode = request.StudentCode;
            student.AcademicLevel = request.AcademicLevel;
            student.DepartmentId = request.DepartmentId;

            // 3. حفظ التعديلات للجدولين معاً كـ Transaction واحدة
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}