namespace ATLASDomain.DTO.Student
{
    public  class UpdateStudentDto
    {
       public required string StudentCode { get; set; }
        public string? FullName { get; set; }
        public string? AcademicId { get; set; }
        public int? AcademicLevel { get; set; }
        public int? DepartmentId { get; set; }
    }
}
