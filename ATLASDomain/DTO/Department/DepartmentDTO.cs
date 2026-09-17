namespace ATLASDomain.DTO.Department
{
    public class DepartmentDTO
    {
        public int Id { get; set; }
        public string? DepartmentName { get; set; }
        
        public int NumOfStudents { get; set; }

        public int NumOfDoctors { get; set; }
        public int NumOfTeacherAssistants { get; set; }
    }
}
