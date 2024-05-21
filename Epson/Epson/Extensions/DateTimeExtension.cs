namespace Epson.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime AddWorkingDays(this DateTime originalDate, int workingDays)
        {
            int direction = workingDays < 0 ? -1 : 1;
            DateTime resultDate = originalDate;

            while (workingDays != 0)
            {
                resultDate = resultDate.AddDays(direction);

                if (resultDate.DayOfWeek != DayOfWeek.Saturday && resultDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    workingDays -= direction;
                }
            }

            return resultDate;
        }
    }

}
