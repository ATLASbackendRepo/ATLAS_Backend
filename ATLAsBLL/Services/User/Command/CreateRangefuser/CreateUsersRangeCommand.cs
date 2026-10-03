using ATLASDomain.Entities;
using MediatR;
using System.Collections.Generic;

namespace ATLAsBLL.Features.Users.Commands.CreateUsersRange
{
    /// <summary>
    /// Create Users Range Command.
    /// </summary>
    public record CreateUsersRangeCommand(List<UserItemDto> Users) : ICommand<string>, IWriteCommand;
    /// <summary>
    /// User Item Dto.
    /// </summary>
    public record UserItemDto(string FullName, string Password, string AcademicId, enUserRole Role);
}
