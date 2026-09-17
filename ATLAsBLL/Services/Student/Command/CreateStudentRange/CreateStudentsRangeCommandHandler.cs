using ATLAsBLL.Events;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using ExcelDataReader;
using MediatR;
using System.Text;

namespace ATLAsBLL.Features.Students.Commands.CreateStudentsRange
{
    public class CreateStudentsRangeCommandHandler
        : IRequestHandler<CreateStudentsRangeCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;


        public CreateStudentsRangeCommandHandler(
            IUnitOfWork unitOfWork,
            IPublisher publisher)
        {
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<bool> Handle(
            CreateStudentsRangeCommand request,
            CancellationToken cancellationToken)
        {
            Encoding.RegisterProvider(
                CodePagesEncodingProvider.Instance);

            try
            {
                var students = await ReadStudentsFromExcelAsync(
                    request.filePath,
                    cancellationToken);

                if (students.Count == 0)
                    return true;

               
                await _unitOfWork.Students.AddRangeAsync(students);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish events AFTER SaveChanges
                foreach (var student in students)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await _publisher.Publish(
                        new StudentCreatedDomainEvent(student.StudentId),
                        cancellationToken);
                }

                return true;
            }
            finally
            {
                DeleteExcelFile(request.filePath);
            }
        }

        private async Task<List<Student>> ReadStudentsFromExcelAsync(
            string filePath,
            CancellationToken cancellationToken)
        {
            var students = new List<Student>();

            
           
            var institutionCache =
                new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            var departmentCache =
                new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

          
            var academicIds =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using var stream = File.Open(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            using var reader =
                ExcelReaderFactory.CreateReader(stream);

            do
            {
                bool isHeader = true;

                while (reader.Read())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Skip header
                    if (isHeader)
                    {
                        isHeader = false;
                        continue;
                    }

                    var institutionName =
                        GetCellValue(reader, 0);

                    var departmentName =
                        GetCellValue(reader, 1);

                    var fullName =
                        GetCellValue(reader, 2);

                    var level =
                        GetCellValue(reader, 3);

                    var academicNumber =
                        GetCellValue(reader, 4);

                   
                    if (string.IsNullOrWhiteSpace(fullName) &&
                        string.IsNullOrWhiteSpace(academicNumber))
                    {
                        continue;
                    }

                    ValidateRequiredData(
                        institutionName,
                        departmentName,
                        fullName,
                        level,
                        academicNumber);

                   
                    if (!academicIds.Add(academicNumber))
                    {
                        throw new InvalidDataException(
                            $"Duplicate academic number '{academicNumber}' found in Excel file.");
                    }

                 
                    int institutionId;

                    if (!institutionCache.TryGetValue(
                            institutionName,
                            out institutionId))
                    {
                        institutionId =
                            await _unitOfWork.Institutions
                                .GetInstitutionWithNameAsync(institutionName);

                        if (institutionId <= 0)
                        {
                            throw new InvalidDataException(
                                $"Institution '{institutionName}' was not found.");
                        }

                        institutionCache[institutionName] = institutionId;
                    }

                   
                    var departmentCacheKey =
                        $"{institutionId}:{departmentName}";

                    int departmentId;

                    if (!departmentCache.TryGetValue(
                            departmentCacheKey,
                            out departmentId))
                    {
                        departmentId =
                            await _unitOfWork.Departments
                                .GetDepartmentWithnameAsync(
                                    departmentName,
                                    institutionId);

                        if (departmentId <= 0)
                        {
                            throw new InvalidDataException(
                                $"Department '{departmentName}' was not found in institution '{institutionName}'.");
                        }

                        departmentCache[departmentCacheKey] =
                            departmentId;
                    }

                    // Parse academic level
                    int academicLevel =
                        ParseAcademicLevel(level);

                    var student = new Student
                    {
                        InstitutionID = institutionId,
                        DepartmentId = departmentId,
                        AcademicId = academicNumber,
                        AcademicLevel = academicLevel,
                        IsActive = true,

                        User = new User
                        {
                            AcademicId = academicNumber,
                            FullName = fullName,
                            DepartmentId = departmentId,
                            InstitutionID = institutionId,
                            Password = BCrypt.Net.BCrypt.HashPassword("12345678"),
                            Role = enUserRole.Student,
                            IsActive = true,
                            MustChangePassword = true
                        }
                    };

                    students.Add(student);
                }

            } while (reader.NextResult());

            return students;
        }

        private static string GetCellValue(
            IExcelDataReader reader,
            int columnIndex)
        {
            return reader.GetValue(columnIndex)?
                .ToString()?
                .Trim() ?? string.Empty;
        }


        private static void ValidateRequiredData(
            string institutionName,
            string departmentName,
            string fullName,
            string level,
            string academicNumber)
        {
            if (string.IsNullOrWhiteSpace(institutionName))
                throw new InvalidDataException(
                    "Institution name is required.");

            if (string.IsNullOrWhiteSpace(departmentName))
                throw new InvalidDataException(
                    $"Department is required for student '{fullName}'.");

            if (string.IsNullOrWhiteSpace(fullName))
                throw new InvalidDataException(
                    $"Student name is required for academic number '{academicNumber}'.");

            if (string.IsNullOrWhiteSpace(academicNumber))
                throw new InvalidDataException(
                    $"Academic number is required for student '{fullName}'.");

            if (string.IsNullOrWhiteSpace(level))
                throw new InvalidDataException(
                    $"Academic level is required for student '{fullName}'.");
        }

        private static int ParseAcademicLevel(string level)
        {
            return level.Trim().ToLowerInvariant() switch
            {
                "level 1" or "1" => 1,
                "level 2" or "2" => 2,
                "level 3" or "3" => 3,
                "level 4" or "4" => 4,

                _ => throw new InvalidDataException(
                    $"Invalid academic level '{level}'.")
            };
        }

        private static void DeleteExcelFile(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            try
            {
                File.Delete(filePath);
            }
            catch
            {
                // Don't hide the original import exception
            }
        }
    }
}
