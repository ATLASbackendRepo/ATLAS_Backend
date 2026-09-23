namespace ATLAS.Middleware.Alert
{
    /// <summary>
    /// Discord Options.
    /// </summary>
    public class DiscordOptions
    {
        /// <summary>
        /// Section Name.
        /// </summary>
        public const string SectionName = "Discord";
        /// <summary>
        /// Enabled.
        /// </summary>
        public bool Enabled { get; set; } = false;
        /// <summary>
        /// Webhook Url.
        /// </summary>
        public string WebhookUrl { get; set; } = null!;

     
    }
}
