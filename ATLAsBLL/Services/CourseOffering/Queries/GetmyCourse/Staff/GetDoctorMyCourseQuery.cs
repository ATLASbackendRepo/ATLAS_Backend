using MediatR;
using System.Collections.Generic;
using ATLASDomain.DTO.CourseOffring;

namespace ATLAsBLL.Services.CourseOffering.Queries.GetmyCourse.Doctor
{

    public record GetStaffMyCourseQuery(int StaffId) : IQuery<IEnumerable<DoctorCourseOfferingDTO>>;
}