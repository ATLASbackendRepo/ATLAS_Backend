public class InstitutionDto
{
    public int InstitutionID { get; set; }
    public string Name { get; set; }
}

public class CreateInstitutionDto
{
    public string Name { get; set; }
}

public class UpdateInstitutionDto
{
    public int InstitutionID { get; set; }
    public string Name { get; set; }
}
