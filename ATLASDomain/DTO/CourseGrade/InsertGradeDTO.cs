

namespace ATLASDomain.DTO.CourseGrade
{
    /// <summary>
    /// Insert Grade DTO.
    /// </summary>
    public  class InsertGradeDTO
    {
      /// <summary>
      /// Module Id.
      /// </summary>
      public  int ModuleId { get; set; }
       /// <summary>
       /// Enrollment Id.
       /// </summary>
       public int EnrollmentId { get; set; }
     /// <summary>
     /// Student Degree.
     /// </summary>
     public decimal StudentDegree { get; set; }
     /// <summary>
     /// User Id.
     /// </summary>
     public int UserId { get; set; }
    }
}
