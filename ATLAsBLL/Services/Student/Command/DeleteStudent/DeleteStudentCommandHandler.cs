using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Students.Commands.DeleteStudent
{
    public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteStudentCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
        {
            // جلب الطالب للتأكد من وجوده وجلب الـ User الخاص به
            var student = await _unitOfWork.Students.GetStudentWithUserAsync(request.StudentId);

            if (student == null)
            {
                throw new System.Collections.Generic.KeyNotFoundException($"Student with ID {request.StudentId} does not exist.");
            }

            var user = student.User;

            // حذف الطالب من الـ Repository الخاص به
            student.IsActive = false; // أو يمكنك استخدام _unitOfWork.Students.Remove(student) إذا كنت تريد الحذف الفعلي

            // حذف الـ User المرتبط بالطالب لمنع تيتيم البيانات (Orphaned Data)
            if (user != null)
            {
               user.IsActive = false; // أو يمكنك استخدام _unitOfWork.Users.Remove(user) إذا كنت تريد الحذف الفعلي
            }

            // تأكيد الحفظ في قاعدة البيانات دفعة واحدة
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}