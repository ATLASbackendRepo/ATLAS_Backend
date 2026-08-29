namespace ATLAS.Middleware.Alert
{
    public class EmailOptions
    {
        public const string SectionName = "Email";
        public string SmtpHost { get; set; } = null!;
        public int SmtpPort { get; set; }
        public string SenderEmail { get; set; } = null!;
        public string SenderPassword { get; set; } = null!;
        public string RecipientEmail { get; set; } = null!; // إيميل الأدمن اللي هيستقبل التنبيهات
        public bool EnableSsl { get; set; } = true;
    }
}
