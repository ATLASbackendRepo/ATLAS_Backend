namespace ATLAS.Middleware.Alert
{
    /// <summary>
    /// Email Options.
    /// </summary>
    public class EmailOptions
    {
        /// <summary>
        /// Section Name.
        /// </summary>
        public const string SectionName = "Email";
        /// <summary>
        /// Smtp Host.
        /// </summary>
        public string SmtpHost { get; set; } = null!;
        /// <summary>
        /// Smtp Port.
        /// </summary>
        public int SmtpPort { get; set; }
        /// <summary>
        /// Sender Email.
        /// </summary>
        public string SenderEmail { get; set; } = null!;
        /// <summary>
        /// Sender Password.
        /// </summary>
        public string SenderPassword { get; set; } = null!;
        /// <summary>
        /// Recipient Email.
        /// </summary>
        public string RecipientEmail { get; set; } = null!; // إيميل الأدمن اللي هيستقبل التنبيهات
        /// <summary>
        /// Enable Ssl.
        /// </summary>
        public bool EnableSsl { get; set; } = true;
    }
}
