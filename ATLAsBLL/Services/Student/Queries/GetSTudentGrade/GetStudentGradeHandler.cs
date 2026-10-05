using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.Student;
using ATLASDomain.Entities;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.Student.Queries.GetSTudentGrade
{
    /// <summary>
    /// Get Student Grade Handler.
    /// </summary>
    internal class GetStudentGradeHandler : IRequestHandler<GetStudentGradeQuery, StudentGradeDto>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;
        public GetStudentGradeHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<StudentGradeDto> Handle(GetStudentGradeQuery request, CancellationToken cancellationToken)
        {
            var student = await _unitOfWork.Students.GetStudentWithModeAsync(request.CourseOfferingID, request.ModuleId, request.StudentCode, cancellationToken);
            if (student == null) return null!;
            return student;
        }
    
    }
}
