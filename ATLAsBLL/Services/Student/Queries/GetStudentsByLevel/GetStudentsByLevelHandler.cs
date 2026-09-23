using ATLAsBLL.Services.Student.Queries.GetStudentByCode;
using ATLAsBLL.Services.Student.Queries.GetStudentsByLevel;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.Student;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.Student.Queries.GetallStudent
{
    /// <summary>
    /// Get Studentsby Level Handler.
    /// </summary>
    public class GetStudentsbyLevelHandler : IRequestHandler<GetStudentsByLevelQuery, IEnumerable<StudentDTO>>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public GetStudentsbyLevelHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<IEnumerable<StudentDTO>> Handle(GetStudentsByLevelQuery request, CancellationToken cancellationToken)
        {

            var students = await _unitOfWork.Students.GetAllStudentWithLevelAsync(request.Level);

            if (students == null) return Enumerable.Empty<StudentDTO>();

            return students.Select(s => s.ToDto());

        }
    }
}
