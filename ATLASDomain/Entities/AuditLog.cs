namespace ATLAsDomain.Entities
{
    public class AuditLog
    {
        public long Id { get; set; }

        public int? UserId { get; set; }

        public string Action { get; set; } = null!;

        public string? EntityType { get; set; }

        public string? EntityId { get; set; }

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }

        public string? IpAddress { get; set; }

        public string? CorrelationId { get; set; }

        public bool Success { get; set; }

        public DateTime TimestampUtc { get; set; }
    }
}