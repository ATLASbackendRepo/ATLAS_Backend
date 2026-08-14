
namespace ATLASDomain.Entities
{
    public  class Institution
    {
        public int InstitutionID { get; set; }
        public string name { get; set; }

        public ICollection<Student> Students { get; set; } = new List<Student>();

    }
}
