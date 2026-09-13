using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.Student;
using MediatR;

namespace ATLAsBLL.Services.Student.Queries.GetSTudentGrade;

internal sealed class GetStudentGradeForUserQueryHandler : IRequestHandler<GetStudentGradeForUserQuery, StudentGradeDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetStudentGradeForUserQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<StudentGradeDto?> Handle(GetStudentGradeForUserQuery request, CancellationToken cancellationToken)
    {
        var student = await _unitOfWork.Students.GetStudentByUserIDAsync(request.UserId);
        return student is null
            ? null
            : await _unitOfWork.Students.GetStudentWithModeAsync(
                request.CourseOfferingId, request.ModuleId, student.StudentCode);
    }
}
