using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using ATLASDomain.Entities;
using ClosedXML.Excel;

namespace ATLAsBLL.Services.Common
{
    /// <summary>
    /// Represents a generated student or staff credential record for export.
    /// </summary>
    public sealed record StudentCredentialRecord(
        string AcademicId,
        string FullName,
        string TemporaryPassword,
        DateTime ExpiresAtUtc,
        string Role = "Student");

    /// <summary>
    /// Result returned after importing a range of student records.
    /// </summary>
    public sealed record StudentImportResult(
        int TotalImported,
        byte[] ExcelFileBytes,
        DateTime ExpiresAtUtc);

    /// <summary>
    /// Result returned after creating a single student account.
    /// </summary>
    public sealed record CreateStudentResult(
        int StudentId,
        string AcademicId,
        string FullName,
        string TemporaryPassword,
        DateTime ExpiresAtUtc,
        byte[] ExcelFileBytes);

    /// <summary>
    /// Result returned after creating a single staff user account.
    /// </summary>
    public sealed record CreateUserResult(
        int UserId,
        string AcademicId,
        string FullName,
        enUserRole Role,
        string TemporaryPassword,
        DateTime ExpiresAtUtc,
        byte[] ExcelFileBytes);

    /// <summary>
    /// Provides cryptographic temporary password generation and in-memory
    /// protected spreadsheet exports for initial account onboarding (BE-07 / BE-08).
    /// </summary>
    public static class CredentialExportService
    {
        private const int OneTimePasswordBytes = 18;

        /// <summary>
        /// Generates a cryptographically secure random one-time password.
        /// </summary>
        /// <returns>A 24-character URL-safe Base64 random token.</returns>
        public static string GenerateTemporaryPassword()
        {
            Span<byte> buffer = stackalloc byte[OneTimePasswordBytes];
            RandomNumberGenerator.Fill(buffer);

            return Convert.ToBase64String(buffer)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        /// <summary>
        /// Builds a protected in-memory Excel workbook containing initial credentials.
        /// Plaintext credentials are never written to disk on the server.
        /// </summary>
        /// <param name="records">Credential records to export.</param>
        /// <returns>The raw Excel workbook file bytes.</returns>
        public static byte[] GenerateCredentialsWorkbook(IEnumerable<StudentCredentialRecord> records)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Credentials");

            // Header Row
            worksheet.Cell(1, 1).Value = "Academic ID";
            worksheet.Cell(1, 2).Value = "Full Name";
            worksheet.Cell(1, 3).Value = "Temporary Password";
            worksheet.Cell(1, 4).Value = "Expires At (UTC)";
            worksheet.Cell(1, 5).Value = "Account Role";
            worksheet.Cell(1, 6).Value = "Security Instructions";

            var headerRange = worksheet.Range(1, 1, 1, 6);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E79");
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var row = 2;
            foreach (var record in records)
            {
                worksheet.Cell(row, 1).Value = record.AcademicId;
                worksheet.Cell(row, 2).Value = record.FullName;
                worksheet.Cell(row, 3).Value = record.TemporaryPassword;
                worksheet.Cell(row, 4).Value = record.ExpiresAtUtc.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
                worksheet.Cell(row, 5).Value = record.Role;
                worksheet.Cell(row, 6).Value = "Must change password at first login. Credential expires in 72 hours.";

                // Highlight temporary password column with subtle background
                worksheet.Cell(row, 3).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFF2CC");
                worksheet.Cell(row, 3).Style.Font.FontName = "Consolas";

                row++;
            }

            worksheet.Columns().AdjustToContents();
            worksheet.SheetView.FreezeRows(1);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}