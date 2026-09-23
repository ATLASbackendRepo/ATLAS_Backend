using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.Student;
using ATLASDomain.Entities;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.Student.Queries.GetStudentsByOferringCoures
{
    /// <summary>
    /// Get Students By Oferring Coures Handler.
    /// </summary>
    public  class GetStudentsByOferringCouresHandler : IRequestHandler<GetStudentsByOferringCouresQuery, IEnumerable<StudentDTO>>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;
        public GetStudentsByOferringCouresHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<IEnumerable<StudentDTO>> Handle(GetStudentsByOferringCouresQuery request, CancellationToken cancellationToken)
        {

            var students = await _unitOfWork.Students.GetAllStudentWithCoursefferingAsync(request.CourseOfferingID);

            if (students == null) return Enumerable.Empty<StudentDTO>();


            return students.Select(s => s.ToDto());

        }

    }
}
