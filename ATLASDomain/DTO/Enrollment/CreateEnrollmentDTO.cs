namespace ATLASDomain.DTO.Enrollment
{
    public  record BulkCreateEnrollmentDTO
    {
        public int CourseID { get; set;  }

        public string AcademicYear { get; set; }
        public string SemesterName { get; set; } 
        public List<int> DoctorIDs { get; set; }

        public List<int> TeacherAssistS { get; set; }

        public int DepartmentID { get; set; }

        public int level { get; set;}


    }
}
