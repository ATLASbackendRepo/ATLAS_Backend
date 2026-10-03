using ATLASDomain.DTO.CourseGrade;
using MediatR;
using System.Collections.Generic;

namespace ATLAsBLL.Features.CourseGrades.Commands.CreateCourseGradesRange
{
    /// <summary>
    /// Assign Course Grades Range Command.
    /// </summary>
    public record AssignCourseGradesRangeCommand(
     string  FilePath , int CourseOfferingID , int ModelID ,int UserID
    ) : ICommand<bool>, IWriteCommand;
}
