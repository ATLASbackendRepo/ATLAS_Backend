namespace ATLASDomain.DTO.Enrollment
{
    /// <summary>
    /// Bulk Create Enrollment DTO.
    /// </summary>
    public  record BulkCreateEnrollmentDTO
    {
        /// <summary>
        /// Course ID.
        /// </summary>
        public int CourseID { get; set;  }

        /// <summary>
        /// Academic Year.
        /// </summary>
        public string AcademicYear { get; set; }
        /// <summary>
        /// Semester Name.
        /// </summary>
        public string SemesterName { get; set; } 
        /// <summary>
        /// Doctor IDs.
        /// </summary>
        public List<int> DoctorIDs { get; set; }

        /// <summary>
        /// Teacher Assist S.
        /// </summary>
        public List<int> TeacherAssistS { get; set; }

        /// <summary>
        /// Department ID.
        /// </summary>
        public int DepartmentID { get; set; }

        /// <summary>
        /// level.
        /// </summary>
        public int level { get; set;}


    }
}
