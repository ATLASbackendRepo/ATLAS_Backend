using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLASDomain.DTO.CourseOffring
{
    public class DoctorCourseOfferingDTO
    {
        public int CourseOfferingID { get; set; }
        public int CourseOfferingHours { get; set; }
        public string CourseName { get; set; } = null!;
        public string Semester { get; set; } = null!;
     

    }
}
