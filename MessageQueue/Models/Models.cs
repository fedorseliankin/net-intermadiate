namespace MessageQueue.Models
{
    public class NotificationMessage
    {
        public Guid TrackingId { get; set; }
        public string OperationName { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public NotificationParameters Params { get; set; }
        public string Content { get; set; }
    }

    public class NotificationParameters
    {
        public string CustomerEmail { get; set; }
        public string CustomerName { get; set; }
    }
}
