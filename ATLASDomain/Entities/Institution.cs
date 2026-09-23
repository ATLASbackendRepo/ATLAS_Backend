
namespace ATLASDomain.Entities
{
    /// <summary>
    /// Institution.
    /// </summary>
    public  class Institution
    {
        /// <summary>
        /// Institution ID.
        /// </summary>
        public int InstitutionID { get; set; }
        /// <summary>
        /// name.
        /// </summary>
        public string? name { get; set; }

        /// <summary>
        /// Students.
        /// </summary>
        public ICollection<Student> Students { get; set; } = new List<Student>();

        /// <summary>
        /// Users.
        /// </summary>
        public ICollection<User> Users { get; set; } = new List<User>();



        /// <summary>
        /// Departments.
        /// </summary>
        public ICollection<Department>? Departments { get; set; }




    }
}
