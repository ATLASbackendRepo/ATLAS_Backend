/// <summary>
/// Institution Dto.
/// </summary>
public class InstitutionDto
{
    /// <summary>
    /// Institution ID.
    /// </summary>
    public int InstitutionID { get; set; }
    /// <summary>
    /// Name.
    /// </summary>
    public string Name { get; set; }
}

/// <summary>
/// Create Institution Dto.
/// </summary>
public class CreateInstitutionDto
{
    /// <summary>
    /// Name.
    /// </summary>
    public string Name { get; set; }
}

/// <summary>
/// Update Institution Dto.
/// </summary>
public class UpdateInstitutionDto
{
    /// <summary>
    /// Institution ID.
    /// </summary>
    public int InstitutionID { get; set; }
    /// <summary>
    /// Name.
    /// </summary>
    public string Name { get; set; }
}
