namespace ATLASDomain.DTO.Department
{
    /// <summary>
    /// Department DTO.
    /// </summary>
    public class DepartmentDTO
    {
        /// <summary>
        /// Id.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Department Name.
        /// </summary>
        public string? DepartmentName { get; set; }
        
        /// <summary>
        /// Num Of Students.
        /// </summary>
        public int NumOfStudents { get; set; }

        /// <summary>
        /// Num Of Doctors.
        /// </summary>
        public int NumOfDoctors { get; set; }
        /// <summary>
        /// Num Of Teacher Assistants.
        /// </summary>
        public int NumOfTeacherAssistants { get; set; }
    }
}
