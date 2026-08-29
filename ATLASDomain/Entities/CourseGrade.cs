namespace ATLASDomain.Entities
{
    public class CourseGrade
    {
        public int CG_ID { get; set; }
        public int EnrollmentID { get; set; }
        public int ModuleID { get; set; } 
        public decimal StudentDegree { get; set; } 
        public int CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
       
     
        public Enrollment? Enrollment { get; set; }
        public AssessmentModule? AssessmentModule { get; set; }

        public ICollection<AssessmentModule> AssessmentModules { get; set; } = new List<AssessmentModule>();
    }
}
