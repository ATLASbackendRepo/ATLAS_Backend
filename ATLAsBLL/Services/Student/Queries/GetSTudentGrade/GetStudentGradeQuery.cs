using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATLASDomain.DTO.Student;
using MediatR;

namespace ATLAsBLL.Services.Student.Queries.GetSTudentGrade
{
    public record GetStudentGradeQuery(int CourseOfferingID, int ModuleId, string StudentCode) : IQuery<StudentGradeDto>
    {
    }
}
