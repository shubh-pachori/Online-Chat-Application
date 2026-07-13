using System;
using ChatApp.Core.Interfaces;

namespace ChatApp.Infrastructure.Services
{
    public class TimeHelper : ITimeHelper
    {
        private readonly TimeZoneInfo _istZone;

        public TimeHelper()
        {
            try
            {
                // Linux/macOS
                _istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            }
            catch (TimeZoneNotFoundException)
            {
                // Windows fallback
                _istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            }
        }

        public DateTime GetIstTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _istZone);
        }

        public DateTime ConvertToIst(DateTime utcTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, _istZone);
        }
    }
}
