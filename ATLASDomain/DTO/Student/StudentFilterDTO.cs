namespace ATLASDomain.DTO.Student
{
    public class StudentFilterDTO
    {
        public string? Search { get; set; }
        public int? DepartmentId { get; set; }
        public int? Level { get; set; }
        public bool? Status { get; set; }
    }
}
