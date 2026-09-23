using ATLASDomain.DTO.Staff;
using ATLASDomain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsBLL.Services.User.qeusies.GetallStaff
{
    /// <summary>
    /// Getall Staff.
    /// </summary>
    public sealed record GetallStaff(enUserRole role, StaffFilterDTO filter) : IQuery<IEnumerable<StaffDto>>;

}
