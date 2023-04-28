namespace Epson.Job
{
    public class SendEmailBackgroundProcessSettings
    {
        //todo: configure each email sending batch size 
        public int BatchSize { get; set; }
        public int IntervalMinutes { get; set; }
    }

}
