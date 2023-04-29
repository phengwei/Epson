namespace Epson.Model.Email
{
    public class EmailQueueModel
    {
        public int Id { get; set; }
        public int Priority { get; set; }
        public string FromEmail { get; set; }
        public string ToEmail { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string AttachmentName { get; set; }
        public DateTime ScheduleTime { get; set; }
        public int SendAttempts { get; set; }
        public DateTime? SentTime { get; set; }
        public string EmailAccountUserName { get; set; }
    }
}
