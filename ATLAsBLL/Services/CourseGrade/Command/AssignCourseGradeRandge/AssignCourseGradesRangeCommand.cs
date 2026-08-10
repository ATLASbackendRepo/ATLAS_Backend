using ATLASDomain.DTO.CourseGrade;
using MediatR;
using System.Collections.Generic;

namespace ATLAsBLL.Features.CourseGrades.Commands.CreateCourseGradesRange
{
    public record AssignCourseGradesRangeCommand(
      List<InsertGradeDTO> GradeDTO
    ) : ICommand<bool>;

   
}