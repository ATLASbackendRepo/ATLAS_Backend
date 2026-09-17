namespace ATLASDomain.DTO.Course
{
    public  class CourseDTO
    {
        public int Id { get; set; }
        public string? CourseName { get; set; }
        public int CreditHours { get; set; }
        public bool IsPractical { get; set; }
    }
}
