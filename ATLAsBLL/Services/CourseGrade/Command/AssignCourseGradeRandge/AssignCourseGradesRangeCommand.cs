using ATLASDomain.DTO.CourseGrade;
using MediatR;
using System.Collections.Generic;

namespace ATLAsBLL.Features.CourseGrades.Commands.CreateCourseGradesRange
{
    public record AssignCourseGradesRangeCommand(
     string  FilePath , int CourseOfferingID , int ModelID ,int UserID
    ) : ICommand<bool>;

   
}