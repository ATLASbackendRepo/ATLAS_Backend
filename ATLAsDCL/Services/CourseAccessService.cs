using ATLAsDCL.Data;
using ATLASDomain.Entities;
using ATLASDomain.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ATLAsDCL.Services
{
    public class CourseAccessService : ICourseAccessService
    {
        private readonly AtLASDbContext _context;
        public CourseAccessService(AtLASDbContext context) => _context = context;

        public async Task<bool> CanSendAsync(int userId, int courseOfferingId)
        {
            // Doctor?
            var isDoctor = await _context.CourseOfferingDoctors.AnyAsync(x => x.CourseOfferingID == courseOfferingId && x.DoctorID == userId);
            if (isDoctor) return true;

            // TA?
            var isTa = await _context.CourseOfferingTAs.AnyAsync(x => x.CourseOfferingID == courseOfferingId && x.TeacherAssistID == userId);
            if (isTa) return true;

            return false;
        }

        public async Task<bool> CanReadAsync(int userId, int courseOfferingId)
        {
            // Doctor or TA
            if (await _context.CourseOfferingDoctors.AnyAsync(x => x.CourseOfferingID == courseOfferingId && x.DoctorID == userId))
                return true;

            if (await _context.CourseOfferingTAs.AnyAsync(x => x.CourseOfferingID == courseOfferingId && x.TeacherAssistID == userId))
                return true;

            // Student with enrollment
            var isEnrolled = await _context.Enrollments.AnyAsync(e => e.CourseOfferingID == courseOfferingId && e.Student.UserId == userId);
            if (isEnrolled) return true;

            return false;
        }
    }
}
