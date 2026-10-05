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
    /// Get All Students Query Handler.
    /// </summary>
    public class GetAllStudentsQueryHandler : IRequestHandler<GetAllStudentsQuery, IEnumerable<StudentDTO>>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public GetAllStudentsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<IEnumerable<StudentDTO>> Handle(GetAllStudentsQuery request, CancellationToken cancellationToken)
        {
          
            var students = await _unitOfWork.Students.GetAllStudents(request.StudentFilter, cancellationToken);

            if (students == null) return Enumerable.Empty<StudentDTO>();

          
           

            
            return students;
        }
    }
}
