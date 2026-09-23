namespace ATLASDomain.DTO.Student
{
    /// <summary>
    /// Student DTO.
    /// </summary>
    public class StudentDTO
    {
        /// <summary>
        /// Student Code.
        /// </summary>
        public string StudentCode { get; set; } = null!;
        /// <summary>
        /// Student Name.
        /// </summary>
        public string StudentName { get; set; } = null!; 
        /// <summary>
        /// Academic Id.
        /// </summary>
        public string AcademicId { get; set; } = null!;
        /// <summary>
        /// Academic Level.
        /// </summary>
        public int AcademicLevel { get; set; } 

        /// <summary>
        /// Departmentname.
        /// </summary>
        public string Departmentname { get; set; } = null!;

        /// <summary>
        /// Is Active.
        /// </summary>
        public bool IsActive { get; set; }

    }
}
