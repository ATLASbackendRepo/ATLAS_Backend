using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;

namespace ATLAsBLL.Features.Enrollments.Commands.CreateEnrollment
{
    public class CreateEnrollmentCommandHandler : IRequestHandler<CreateEnrollmentCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateEnrollmentCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> Handle(CreateEnrollmentCommand request, CancellationToken cancellationToken)
        {
            // 1. التحقق من وجود الطالب
            var studentExists = await _unitOfWork.Students.GetByIdAsync(request.StudentId);
            if (studentExists == null)
                throw new System.Collections.Generic.KeyNotFoundException($"Student with ID {request.StudentId} does not exist.");

            // 2. التحقق من وجود المادة المطروحة
            var offeringExists = await _unitOfWork.CourseOfferings.GetByIdAsync(request.CourseOfferingId);
            if (offeringExists == null)
                throw new System.Collections.Generic.KeyNotFoundException($"Course Offering with ID {request.CourseOfferingId} does not exist.");

            // 3. التحقق من عدم وجود تسجيل مسبق لنفس الطالب في نفس المادة (Custom Method في الـ Repository)
            var isAlreadyEnrolledinthisSemester  = await _unitOfWork.Enrollments.IsStudentEnrolledAsync(request.StudentId, request.CourseOfferingId, DateTime.UtcNow);
            if (isAlreadyEnrolledinthisSemester)
                throw new InvalidOperationException("Student is already enrolled in this course offering.");

            // 4. إنشاء كائن التسجيل
            var enrollment = new Enrollment
            {
                StudentID = request.StudentId,
                CourseOfferingID = request.CourseOfferingId,
                EnrollmentDate = DateTime.UtcNow // تسجيل وقت العملية تلقائياً
            };

            await _unitOfWork.Enrollments.AddAsync(enrollment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return enrollment.EnrollmentID;
        }
    }
}