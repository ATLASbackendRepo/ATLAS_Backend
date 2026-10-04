
using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.Request;
using MediatR;

namespace ATLAsBLL.Services.Request.Qeuries
{

    /// <summary>
    /// Get Student Grade Appeals Query Handler.
    /// </summary>
    public class GetStudentGradeAppealsQueryHandler
        : IRequestHandler<GetStudentGradeAppealsQuery, List<GradeAppealRequestDto>>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public GetStudentGradeAppealsQueryHandler(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<List<GradeAppealRequestDto>> Handle(
            GetStudentGradeAppealsQuery query,
            CancellationToken cancellationToken)
        {

           ATLASDomain.Entities.Student? student = await  _unitOfWork.Students.GetStudentByUserIDAsync(query.UserId, cancellationToken);

            if (student is null)
            {
                throw new KeyNotFoundException(
                    $"Student with ID '{query.UserId}' was not found.");
            }

            return await _unitOfWork.GradeAppealRequests.GetByStudentIdAsync(student.StudentId, cancellationToken);
        }
    }
}
