using ATLAsBLL.Services.Student.Queries.GetStudentByCode;
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
    /// Get Student By Code Handler.
    /// </summary>
    public class GetStudentByCodeHandler : IRequestHandler<GetStudentByCodeQuery, StudentDTO>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public GetStudentByCodeHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<StudentDTO> Handle(GetStudentByCodeQuery request, CancellationToken cancellationToken)
        {

            var student = await _unitOfWork.Students.GetStudentByCodeAsync(request.StudentCode);


            if (student is null)
            {
                throw new KeyNotFoundException(
                    $"Student with code '{request.StudentCode}' was not found.");
            }

            return student.ToDto();

        }
    }
}
