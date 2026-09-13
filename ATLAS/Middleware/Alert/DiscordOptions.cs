namespace ATLAS.Middleware.Alert
{
    public class DiscordOptions
    {
        public const string SectionName = "Discord";
        public bool Enabled { get; set; } = false;
        public string WebhookUrl { get; set; } = null!;

     
    }
}
