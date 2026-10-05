using ATLAS.Domain.Entities;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using ExcelDataReader;
using MediatR;
using System.Security.Cryptography;
using System.Text;

namespace ATLAsBLL.Features.Students.Commands.CreateStudentsRange
{
    /// <summary>
    /// Create Students Range Command Handler.
    /// </summary>
    /// <remarks>
    /// The handler manages its own transaction
    /// (<see cref="ISelfManagedTransactionCommand{TResponse}"/>): the workbook is
    /// parsed and every one-time password is hashed before the transaction opens,
    /// so the import cannot hold database locks while the CPU-bound BCrypt calls
    /// run. The whole file is written with a bounded number of round trips and the
    /// number of rows is capped by <see cref="MaxImportRows"/>.
    /// </remarks>
    public class CreateStudentsRangeCommandHandler
        : IRequestHandler<CreateStudentsRangeCommand, bool>
    {
        /// <summary>
        /// Largest number of data rows a single import may contain.
        /// </summary>
        public const int MaxImportRows = 10_000;

        /// <summary>
        /// Number of random bytes behind one generated one-time password.
        /// </summary>
        private const int OneTimePasswordBytes = 18;

        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;


        public CreateStudentsRangeCommandHandler(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<bool> Handle(
            CreateStudentsRangeCommand request,
            CancellationToken cancellationToken)
        {
            Encoding.RegisterProvider(
                CodePagesEncodingProvider.Instance);

            try
            {
                // Parsing the workbook and hashing a password per row is CPU-bound
                // and needs no database write, so it happens before the transaction
                // opens. The transaction then only covers the two batched inserts.
                var preparedRows = await ReadStudentsFromExcelAsync(
                    request.filePath,
                    cancellationToken);

                if (preparedRows.Count == 0)
                    return true;

                var students = preparedRows
                    .Select(row => row.ToStudent(GenerateOneTimePassword()))
                    .ToList();

                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                try
                {
                    // One round trip for every user and student in the file.
                    await _unitOfWork.Students.AddRangeAsync(students, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // The generated student ids are known now, so every profile is
                    // written in one more round trip. The previous implementation
                    // published a domain event per student and each event ran its
                    // own SaveChanges, which made a file of N rows cost N+1 round
                    // trips.
                    var profiles = students
                        .Select(student => new StudentProfile
                        {
                            StudentID = student.StudentId,
                            CreatedAt = DateTime.UtcNow
                        })
                        .ToList();

                    await _unitOfWork.StudentProfile.AddRangeAsync(profiles, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }

                return true;
            }
            finally
            {
                DeleteExcelFile(request.filePath);
            }
        }

        /// <summary>
        /// Reads and validates every data row of the workbook.
        /// </summary>
        /// <remarks>
        /// Only parsing, validation and read-only lookups happen here. The rows come
        /// back as <see cref="PreparedStudent"/> values, so no password is generated
        /// or hashed until the row cap has been enforced.
        /// </remarks>
        private async Task<List<PreparedStudent>> ReadStudentsFromExcelAsync(
            string filePath,
            CancellationToken cancellationToken)
        {
            var rows = new List<PreparedStudent>();

            
           
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
                                .GetInstitutionWithNameAsync(institutionName, cancellationToken);

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
                                    institutionId, cancellationToken);

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

                    // The cap is checked before a row is accepted and before any
                    // password is hashed, so an oversized file fails fast without
                    // burning minutes of BCrypt work.
                    if (rows.Count >= MaxImportRows)
                    {
                        throw new InvalidDataException(
                            $"The file contains more than {MaxImportRows} students. Split it into smaller files.");
                    }

                    rows.Add(new PreparedStudent(
                        institutionId,
                        departmentId,
                        fullName,
                        academicNumber,
                        academicLevel));
                }

            } while (reader.NextResult());

            return rows;
        }

        /// <summary>
        /// A validated row of the workbook that is ready to become a student.
        /// </summary>
        /// <param name="InstitutionId">Resolved institution id.</param>
        /// <param name="DepartmentId">Resolved department id.</param>
        /// <param name="FullName">Student full name.</param>
        /// <param name="AcademicNumber">Academic id, unique inside the file.</param>
        /// <param name="AcademicLevel">Parsed academic level.</param>
        private sealed record PreparedStudent(
            int InstitutionId,
            int DepartmentId,
            string FullName,
            string AcademicNumber,
            int AcademicLevel)
        {
            /// <summary>
            /// Builds the student and its user account, hashing the supplied
            /// one-time password.
            /// </summary>
            /// <param name="oneTimePassword">
            /// Random password generated for this account.
            /// </param>
            /// <returns>The student, with its user, ready to be inserted.</returns>
            public Student ToStudent(string oneTimePassword)
            {
                return new Student
                {
                    InstitutionID = InstitutionId,
                    DepartmentId = DepartmentId,
                    AcademicId = AcademicNumber,
                    AcademicLevel = AcademicLevel,
                    IsActive = true,

                    User = new User
                    {
                        AcademicId = AcademicNumber,
                        FullName = FullName,
                        DepartmentId = DepartmentId,
                        InstitutionID = InstitutionId,
                        Password = BCrypt.Net.BCrypt.HashPassword(oneTimePassword),
                        Role = enUserRole.Student,
                        IsActive = true,
                        MustChangePassword = true
                    }
                };
            }
        }

        /// <summary>
        /// Generates a cryptographically random one-time password for an account.
        /// </summary>
        /// <remarks>
        /// Only the BCrypt hash is stored, so the plaintext never exists outside this
        /// method and the account cannot be claimed with a known default. The user
        /// has to change the password at the first login, and an administrator hands
        /// out a new one through the admin password-change endpoint when needed.
        /// </remarks>
        /// <returns>A random password of <see cref="OneTimePasswordBytes"/> bytes.</returns>
        private static string GenerateOneTimePassword()
        {
            Span<byte> buffer = stackalloc byte[OneTimePasswordBytes];
            RandomNumberGenerator.Fill(buffer);

            return Convert.ToBase64String(buffer)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        /// <summary>
        /// Get Cell Value.
        /// </summary>
        private static string GetCellValue(
            IExcelDataReader reader,
            int columnIndex)
        {
            return reader.GetValue(columnIndex)?
                .ToString()?
                .Trim() ?? string.Empty;
        }


        /// <summary>
        /// Validate Required Data.
        /// </summary>
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

        /// <summary>
        /// Parse Academic Level.
        /// </summary>
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

        /// <summary>
        /// Delete Excel File.
        /// </summary>
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
