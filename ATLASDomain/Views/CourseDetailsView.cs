

namespace ATLASDomain.Views
{
    /// <summary>
    /// Course Details View.
    /// </summary>
    public class CourseDetailsView
    {

        /// <summary>
        /// student Id.
        /// </summary>
        public int studentId { get; set; }  
        /// <summary>
        /// Course Offering ID.
        /// </summary>
        public int CourseOfferingID { get; set; }
        /// <summary>
        /// C_Name.
        /// </summary>
        public string C_Name { get; set; } = string.Empty;
        /// <summary>
        /// Credit_Hours.
        /// </summary>
        public int Credit_Hours { get; set; }

        /// <summary>
        /// Module Id.
        /// </summary>
        public int ModuleId { get; set; } 
        /// <summary>
        /// studentdegree.
        /// </summary>
        public int studentdegree { get; set; }
        /// <summary>
        /// max Degree.
        /// </summary>
        public int maxDegree { get; set; }

        /// <summary>
        /// Module Name.
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;

    }
}
