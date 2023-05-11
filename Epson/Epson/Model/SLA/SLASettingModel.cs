namespace Epson.Model.SLA
{
    public class SLASettingModel
    {
        public bool IncludeHoliday { get; set; }
        public bool IncludeStaffLeaves { get; set; }
        public bool IncludeWorkingHours { get; set; }
        public int WorkingStartHour { get; set; }
        public int WorkingStartMinute { get; set; }
        public int WorkingEndHour { get; set; }
        public int WorkingEndMinute { get; set; }
        public int DeadlineInHours { get; set; }
    }
}
