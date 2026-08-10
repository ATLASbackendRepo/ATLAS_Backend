

namespace ATLASDomain.Views
{
    public class CourseDetailsView
    {

        public int CourseOfferingID { get; set; }
        public string C_Name { get; set; } = string.Empty;
        public int Credit_Hours { get; set; }

        public int ModuleId { get; set; } 
        public string ModuleName { get; set; } = string.Empty;
        public int maxDegree { get; set; }

        public int studentdegree { get; set; }

        public int studentId { get; set; }  
    }
}
