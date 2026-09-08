using ATLAsBLL.Events;
using ATLAsDCL.UnitOfWork;
using ATLASDomain.Entities;
using MediatR;
using ExcelDataReader;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAsBLL.Features.Students.Commands.CreateStudentsRange
{
    public class CreateStudentsRangeCommandHandler : IRequestHandler<CreateStudentsRangeCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;

        public CreateStudentsRangeCommandHandler(IUnitOfWork unitOfWork, IPublisher publisher)
        {
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<bool> Handle(CreateStudentsRangeCommand request, CancellationToken cancellationToken)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            
            
            var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("12345678");

            try
            {
                using (var stream = System.IO.File.Open(request.filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var studentsList = new List<Student>();

                        do
                        {
                            bool skippedHeader = false;

                            while (reader.Read())
                            {
                                if (!skippedHeader)
                                {
                                    skippedHeader = true;
                                    continue;
                                }

                                var institutionName = reader.GetValue(0)?.ToString()?.Trim() ?? string.Empty;
                                var departmentName  = reader.GetValue(1)?.ToString()?.Trim() ?? string.Empty;
                                var fullName        = reader.GetValue(2)?.ToString()?.Trim() ?? string.Empty;
                                var level           = reader.GetValue(3)?.ToString()?.Trim() ?? string.Empty;
                                var academicNumber  = reader.GetValue(4)?.ToString()?.Trim() ?? string.Empty;

                                // تجنب الصفوف الفارغة في أواخر الملف
                                if (string.IsNullOrWhiteSpace(fullName) && string.IsNullOrWhiteSpace(academicNumber))
                                    continue;

                                // جلب أو إنشاء المعهد والقسم
                                int institutionId = await _unitOfWork.Institutions.GetInstitutionWithNameAsync(institutionName);
                                int departmentId = await _unitOfWork.Departments.GetDepartmentWithnameAsync(departmentName, institutionId);

                                if (institutionId <= 0 || departmentId <= 0 || string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(academicNumber))
                                {
                                    throw new InvalidDataException($"Invalid student data found for '{fullName}' in Excel file.");
                                }

                                int academicLevelInt = level.ToLower() switch
                                {
                                    "level 1" or "1" => 1,
                                    "level 2" or "2" => 2,
                                    "level 3" or "3" => 3,
                                    "level 4" or "4" => 4,
                                    _ => 1
                                };

                                var student = new Student
                                {
                                    InstitutionID = institutionId,
                                    DepartmentId = departmentId,
                                    AcademicId = academicNumber,
                                    AcademicLevel = academicLevelInt,
                                    IsActive = true,
                                    User = new User
                                    {
                                        AcademicId = academicNumber,
                                        FullName = fullName,
                                        DepartmentId = departmentId,
                                        InstitutionID = institutionId,
                                        Password = defaultPasswordHash ,
                                        Role = enUserRole.Student,
                                        IsActive =true
                                    }
                                };

                                studentsList.Add(student);
                            }
                        } while (reader.NextResult());

                        if (studentsList.Count > 0)
                        {
                          
                            await _unitOfWork.Students.AddRangeAsync(studentsList);
                            await _unitOfWork.SaveChangesAsync(cancellationToken);
                        }
                    }
                }

                return true;
            }
            finally
            {
               
                if (System.IO.File.Exists(request.filePath))
                {
                    System.IO.File.Delete(request.filePath);
                }
            }
        }
    }
}