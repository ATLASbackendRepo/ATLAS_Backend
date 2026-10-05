using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.Student;
using ATLASDomain.Entities;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.Student.Queries.GetStudentWithMode
{
    /// <summary>
    /// Get Student With Mode Handeler.
    /// </summary>
    internal class GetStudentWithModeHandeler : IRequestHandler<GetStudentWithModequery, IEnumerable<StudentGradeDto>>

    {

        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;
        public GetStudentWithModeHandeler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<IEnumerable<StudentGradeDto>> Handle(GetStudentWithModequery request, CancellationToken cancellationToken)
        {
            var students = await _unitOfWork.Students.GetAllStudentWithModeAsync(request.CourseOfferingID, request.ModelID, cancellationToken);
            if (students == null) return Enumerable.Empty<StudentGradeDto>();

            return students;
        }
    }
}
