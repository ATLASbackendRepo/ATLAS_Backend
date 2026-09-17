namespace ATLASDomain.DTO.Course
{
    public class UpdateCourseDTO
    {
        public int Id { get; set; }
        public string? CourseName { get; set; }
        public int CreditHours { get; set; }
        public bool IsPractical { get; set; }
    }
}
