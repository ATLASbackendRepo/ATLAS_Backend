namespace ATLAsDomain.Entities
{
    /// <summary>
    /// Audit Log.
    /// </summary>
    public class AuditLog
    {
        /// <summary>
        /// Id.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// User Id.
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Action.
        /// </summary>
        public string Action { get; set; } = null!;

        /// <summary>
        /// Entity Type.
        /// </summary>
        public string? EntityType { get; set; }

        /// <summary>
        /// Entity Id.
        /// </summary>
        public string? EntityId { get; set; }

        /// <summary>
        /// Old Values.
        /// </summary>
        public string? OldValues { get; set; }

        /// <summary>
        /// New Values.
        /// </summary>
        public string? NewValues { get; set; }

        /// <summary>
        /// Ip Address.
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Correlation Id.
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Success.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Timestamp Utc.
        /// </summary>
        public DateTime TimestampUtc { get; set; }
    }
}
