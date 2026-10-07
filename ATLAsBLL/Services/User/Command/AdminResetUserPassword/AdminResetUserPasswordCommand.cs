using ATLASDomain.Entities;
using MediatR;

namespace ATLAsBLL.Services.User.Command.AdminResetUserPassword
{
    /// <summary>
    /// Admin Reset User Password Command.
    /// Allows an Admin or SAdmin to reset a target user's password.
    /// </summary>
    public record AdminResetUserPasswordCommand(
        string TargetAcademicId,
        string NewPassword,
        string ConfirmPassword,
        int PerformingUserId,
        enUserRole PerformingUserRole,
        int? PerformingUserInstitutionId
    ) : IRequest<bool>;
}

