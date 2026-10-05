using ATLAsDCL.Data;
using ATLAsDCL.Repositories;
using ATLASDomain.Entities;
using Xunit;

namespace ATLAS.Tests;

/// <summary>
/// MED-14: the institution and department name lookups behind the Excel import
/// are read-only. An unknown name is reported as 0 so the caller rejects the
/// row instead of silently creating a record from a typo.
/// </summary>
public class InstitutionDepartmentLookupTests
{
    /// <summary>
    /// A name that exists resolves to its id.
    /// </summary>
    [Fact]
    public async Task GetInstitutionWithNameAsync_KnownName_ReturnsItsId()
    {
        await using var context = TestDbContextFactory.Create();
        var institution = new Institution { name = "ATLAS University" };
        context.Institutions.Add(institution);
        await context.SaveChangesAsync();

        var repository = new institutionRepository(context);

        var institutionId = await repository.GetInstitutionWithNameAsync("ATLAS University");

        Assert.Equal(institution.InstitutionID, institutionId);
    }

    /// <summary>
    /// An unknown name reports 0 and never inserts a row.
    /// </summary>
    [Fact]
    public async Task GetInstitutionWithNameAsync_UnknownName_ReturnsZeroAndInsertsNothing()
    {
        await using var context = TestDbContextFactory.Create();
        context.Institutions.Add(new Institution { name = "ATLAS University" });
        await context.SaveChangesAsync();

        var repository = new institutionRepository(context);

        var institutionId = await repository.GetInstitutionWithNameAsync("Typo University");

        Assert.Equal(0, institutionId);
        Assert.Equal(1, context.Institutions.Count());
    }

    /// <summary>
    /// A department name that exists for the institution resolves to its id.
    /// </summary>
    [Fact]
    public async Task GetDepartmentWithnameAsync_KnownNameForTheInstitution_ReturnsItsId()
    {
        await using var context = TestDbContextFactory.Create();
        var department = new Department { D_Name = "General", InstitutionID = 1 };
        context.Departments.Add(department);
        await context.SaveChangesAsync();

        var repository = new DepartmentRepository(context);

        var departmentId = await repository.GetDepartmentWithnameAsync("General", 1);

        Assert.Equal(department.DepartmentId, departmentId);
    }

    /// <summary>
    /// An unknown name reports 0 and never inserts a row.
    /// </summary>
    [Fact]
    public async Task GetDepartmentWithnameAsync_UnknownName_ReturnsZeroAndInsertsNothing()
    {
        await using var context = TestDbContextFactory.Create();
        context.Departments.Add(new Department { D_Name = "General", InstitutionID = 1 });
        await context.SaveChangesAsync();

        var repository = new DepartmentRepository(context);

        var departmentId = await repository.GetDepartmentWithnameAsync("Typo Department", 1);

        Assert.Equal(0, departmentId);
        Assert.Equal(1, context.Departments.Count());
    }

    /// <summary>
    /// The department lookup is scoped to the institution: a name that only
    /// exists somewhere else still reports 0.
    /// </summary>
    [Fact]
    public async Task GetDepartmentWithnameAsync_NameOfAnotherInstitution_ReturnsZero()
    {
        await using var context = TestDbContextFactory.Create();
        context.Departments.Add(new Department { D_Name = "General", InstitutionID = 1 });
        await context.SaveChangesAsync();

        var repository = new DepartmentRepository(context);

        var departmentId = await repository.GetDepartmentWithnameAsync("General", 2);

        Assert.Equal(0, departmentId);
        Assert.Equal(1, context.Departments.Count());
    }
}