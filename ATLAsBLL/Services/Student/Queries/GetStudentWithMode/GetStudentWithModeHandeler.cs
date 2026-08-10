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
    internal class GetStudentWithModeHandeler : IRequestHandler<GetStudentWithModequery, IEnumerable<StudentGradeDto>>

    {

        private readonly IUnitOfWork _unitOfWork;
        public GetStudentWithModeHandeler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IEnumerable<StudentGradeDto>> Handle(GetStudentWithModequery request, CancellationToken cancellationToken)
        {
            var students = await _unitOfWork.Students.GetAllStudentWithModeAsync(request.CourseOfferingID, request.ModelID);
            if (students == null) return Enumerable.Empty<StudentGradeDto>();

            return students;
        }
    }
}
