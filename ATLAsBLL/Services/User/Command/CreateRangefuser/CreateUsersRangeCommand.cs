using ATLASDomain.Entities;
using MediatR;
using System.Collections.Generic;

namespace ATLAsBLL.Features.Users.Commands.CreateUsersRange
{
    
    public record CreateUsersRangeCommand(List<UserItemDto> Users) : ICommand<string>;

    public record UserItemDto(string FullName, string Password, enUserRole Role);
}