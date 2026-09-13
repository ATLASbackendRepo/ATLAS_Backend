namespace ATLASDomain.DTO.Student
{
    public class CreateStudentDTO
    {
        public string FullName { get; set; } = string.Empty;
        public string AcademicId { get; set; } = string.Empty;
        public int AcademicLevel { get; set; }
        public int DepartmentId { get; set; }
        public int InstitutionId { get; set; }
    }
}
