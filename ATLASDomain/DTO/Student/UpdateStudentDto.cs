namespace ATLASDomain.DTO.Student
{
    /// <summary>
    /// Update Student Dto.
    /// </summary>
    public  class UpdateStudentDto
    {
       /// <summary>
       /// Student Code.
       /// </summary>
       public required string StudentCode { get; set; }
        /// <summary>
        /// Full Name.
        /// </summary>
        public string? FullName { get; set; }
        /// <summary>
        /// Academic Id.
        /// </summary>
        public string? AcademicId { get; set; }
        /// <summary>
        /// Academic Level.
        /// </summary>
        public int? AcademicLevel { get; set; }
        /// <summary>
        /// Department Id.
        /// </summary>
        public int? DepartmentId { get; set; }
    }
}
