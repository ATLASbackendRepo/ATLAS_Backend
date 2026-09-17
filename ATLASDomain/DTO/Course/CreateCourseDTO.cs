namespace ATLASDomain.DTO.Course
{
    public class CreateCourseDTO
    {
        public string? CourseName { get; set; }
        public int CreditHours { get; set; }
        public bool IsPractical { get; set; }
    }
}
