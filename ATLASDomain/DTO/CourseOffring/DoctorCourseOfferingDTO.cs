using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLASDomain.DTO.CourseOffring
{
    /// <summary>
    /// Doctor Course Offering DTO.
    /// </summary>
    public class DoctorCourseOfferingDTO
    {
        /// <summary>
        /// Course Offering ID.
        /// </summary>
        public int CourseOfferingID { get; set; }
        /// <summary>
        /// Course Offering Hours.
        /// </summary>
        public int CourseOfferingHours { get; set; }
        /// <summary>
        /// Course Name.
        /// </summary>
        public string CourseName { get; set; } = null!;
        /// <summary>
        /// Semester.
        /// </summary>
        public string Semester { get; set; } = null!;
     

    }
}
