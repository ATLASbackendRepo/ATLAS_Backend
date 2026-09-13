namespace ATLASDomain.DTO.Student
{
    public class StudentDTO
    {
        public string StudentCode { get; set; } = null!;
        public string StudentName { get; set; } = null!; 
        public string AcademicId { get; set; } = null!;
        public int AcademicLevel { get; set; } 

        public string Departmentname { get; set; } = null!;

        public bool IsActive { get; set; }

    }
}