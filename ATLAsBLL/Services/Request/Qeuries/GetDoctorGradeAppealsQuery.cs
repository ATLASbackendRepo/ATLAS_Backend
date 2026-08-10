

    using MediatR;
namespace ATLAsBLL.Services.Request.Qeuries
{

    public record GetDoctorGradeAppealsQuery(int DoctorUserId)
        : IQuery<List<GradeAppealRequestDto>>;
}
