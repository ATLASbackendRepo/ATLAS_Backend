using ClosedXML.Excel;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATLASDomain.DTO.Enrollment;
using ATLASDomain.Entities;
using ATLAsDCL.UnitOfWork;
using ATLAsBLL.Features.Enrollments.Commands.CreateEnrollmentsRange;
using StudentEntity = ATLASDomain.Entities.Student;
using EnrollmentEntity = ATLASDomain.Entities.Enrollment;

namespace ATLAsBLL.Services.Enrollment.Command.CreateEnrollmentRange
{
    /// <summary>
    /// Create Enrollments Range Command Handler.
    /// Processes bulk enrollment requests from uploaded Excel files.
    /// </summary>
    public class CreateEnrollmentsRangeCommandHandler : IRequestHandler<CreateEnrollmentsRangeCommand, BulkEnrollmentResultDTO>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateEnrollmentsRangeCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<BulkEnrollmentResultDTO> Handle(CreateEnrollmentsRangeCommand request, CancellationToken cancellationToken)
        {
            if (request.BulkCreateEnrollment is null)
            {
                throw new ArgumentException("Bulk enrollment details are required.");
            }

            if (request.FileStream is null || request.FileStream.Length == 0)
            {
                throw new InvalidDataException("The uploaded file is empty.");
            }

            var result = new BulkEnrollmentResultDTO();

            // 1. Read and Parse Excel using ClosedXML
            request.FileStream.Position = 0;
            using var workbook = new XLWorkbook(request.FileStream);
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet is null || worksheet.RowsUsed().Count() < 2)
            {
                throw new InvalidDataException("The uploaded Excel file is empty or contains no data rows.");
            }

            var headerRow = worksheet.Row(1);
            int academicIdCol = -1;
            int nameCol = -1;

            int lastCol = headerRow.LastCellUsed()?.Address.ColumnNumber ?? 0;
            for (int col = 1; col <= lastCol; col++)
            {
                var headerName = headerRow.Cell(col).GetString().Trim();
                if (string.Equals(headerName, "AcademicID", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(headerName, "Academic ID", StringComparison.OrdinalIgnoreCase))
                {
                    academicIdCol = col;
                }
                else if (string.Equals(headerName, "Name", StringComparison.OrdinalIgnoreCase))
                {
                    nameCol = col;
                }
            }

            if (academicIdCol == -1 || nameCol == -1)
            {
                throw new InvalidDataException("Excel file must contain 'AcademicID' and 'Name' column headers.");
            }

            var seenInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var parsedRows = new List<(string AcademicId, string Name, int RowNumber)>();

            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                result.TotalRowsProcessed++;

                var academicId = row.Cell(academicIdCol).GetString().Trim();
                var name = row.Cell(nameCol).GetString().Trim();
                int rowNum = row.RowNumber();

                if (string.IsNullOrWhiteSpace(academicId) && string.IsNullOrWhiteSpace(name))
                {
                    result.TotalRowsProcessed--;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(academicId))
                {
                    result.InvalidRowsCount++;
                    result.InvalidRows.Add($"Row {rowNum}: AcademicID is empty.");
                    continue;
                }

                if (!seenInFile.Add(academicId))
                {
                    result.InvalidRowsCount++;
                    result.InvalidRows.Add($"Row {rowNum}: Duplicate AcademicID '{academicId}' in Excel file.");
                    continue;
                }

                parsedRows.Add((academicId, name, rowNum));
            }

            if (parsedRows.Count == 0)
            {
                return result;
            }

            // 2. Validate Semester and CourseOffering
            var dto = request.BulkCreateEnrollment;
            var semester = await _unitOfWork.Semesters.FirstOrDefaultAsync(
                s => s.AcademicYear == dto.AcademicYear && s.SemesterName == dto.SemesterName,
                cancellationToken);

            if (semester is null)
            {
                throw new KeyNotFoundException($"Semester '{dto.SemesterName}' of academic year '{dto.AcademicYear}' was not found.");
            }

            var courseOffering = await _unitOfWork.CourseOfferings.FirstOrDefaultAsync(
                co => co.CourseID == dto.CourseID && co.SemesterID == semester.SemesterID,
                cancellationToken);

            if (courseOffering is null)
            {
                throw new KeyNotFoundException($"Course offering for Course ID {dto.CourseID} in semester '{dto.SemesterName}' of academic year '{dto.AcademicYear}' was not found.");
            }

            // 3. Find matching students in database
            var academicIdsInFile = parsedRows.Select(r => r.AcademicId).ToList();
            var matchingStudents = await _unitOfWork.Students.GetStudentsByAcademicIdsAsync(academicIdsInFile, cancellationToken);

            var studentMap = new Dictionary<string, StudentEntity>(StringComparer.OrdinalIgnoreCase);
            foreach (var student in matchingStudents)
            {
                if (!string.IsNullOrWhiteSpace(student.AcademicId))
                {
                    studentMap[student.AcademicId.Trim()] = student;
                }
                if (student.User != null && !string.IsNullOrWhiteSpace(student.User.AcademicId))
                {
                    studentMap[student.User.AcademicId.Trim()] = student;
                }
            }

            var validStudentsToEnroll = new List<StudentEntity>();

            foreach (var row in parsedRows)
            {
                if (studentMap.TryGetValue(row.AcademicId, out var student))
                {
                    validStudentsToEnroll.Add(student);
                }
                else
                {
                    result.NotFoundCount++;
                    result.NotFoundAcademicIds.Add(row.AcademicId);
                }
            }

            if (validStudentsToEnroll.Count == 0)
            {
                return result;
            }

            // 4. Check for existing enrollments
            var studentIds = validStudentsToEnroll.Select(s => s.StudentId).Distinct().ToList();
            var alreadyEnrolledIds = await _unitOfWork.Enrollments.GetEnrolledStudentIdsAsync(
                courseOffering.CourseOfferingID,
                studentIds,
                cancellationToken);

            var newEnrollments = new List<EnrollmentEntity>();

            foreach (var student in validStudentsToEnroll)
            {
                if (alreadyEnrolledIds.Contains(student.StudentId))
                {
                    result.AlreadyEnrolledCount++;
                    var academicIdStr = !string.IsNullOrWhiteSpace(student.AcademicId)
                        ? student.AcademicId
                        : (student.User?.AcademicId ?? student.StudentId.ToString());
                    result.AlreadyEnrolledAcademicIds.Add(academicIdStr);
                }
                else
                {
                    newEnrollments.Add(new EnrollmentEntity
                    {
                        StudentID = student.StudentId,
                        CourseOfferingID = courseOffering.CourseOfferingID,
                        EnrollmentDate = DateTime.UtcNow,
                        CreatedByUserID = request.CreatedByUserId
                    });
                }
            }

            // 5. Bulk insert new enrollments with transaction consistency
            if (newEnrollments.Count > 0)
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    await _unitOfWork.Enrollments.AddRangeAsync(newEnrollments, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);

                    result.SuccessCount = newEnrollments.Count;
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }
            }

            return result;
        }
    }
}
