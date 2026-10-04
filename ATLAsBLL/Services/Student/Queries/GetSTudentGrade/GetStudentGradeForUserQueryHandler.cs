using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.Student;
using MediatR;

namespace ATLAsBLL.Services.Student.Queries.GetSTudentGrade;

/// <summary>
/// Get Student Grade For User Query Handler.
/// </summary>
internal sealed class GetStudentGradeForUserQueryHandler : IRequestHandler<GetStudentGradeForUserQuery, StudentGradeDto?>
{
    /// <summary>
    /// _unit Of Work.
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    public GetStudentGradeForUserQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <summary>
    /// Handle.
    /// </summary>
    public async Task<StudentGradeDto?> Handle(GetStudentGradeForUserQuery request, CancellationToken cancellationToken)
    {
        var student = await _unitOfWork.Students.GetStudentByUserIDAsync(request.UserId, cancellationToken);
        return student is null
            ? null
            : await _unitOfWork.Students.GetStudentWithModeAsync(
                request.CourseOfferingId, request.ModuleId, student.StudentCode, cancellationToken);
    }
}
