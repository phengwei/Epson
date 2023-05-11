namespace Epson.Model.SLA
{
    public class SLAMetricsModel
    {
        public decimal AverageTimeToResolutionInHours { get; set; }
        public int BreachedTickets { get; set; }
        public int TotalTickets { get; set; }
        public decimal SuccessRate { get; set; }
    }
}
