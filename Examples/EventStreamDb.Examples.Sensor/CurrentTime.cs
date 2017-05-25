using System;
using System.Collections.Generic;
using System.Text;

namespace EventStreamDb.Examples.Sensor
{
    public static class CurrentTime
    {

        private static DateTime _currentTime = DateTime.UtcNow;

        public static DateTime GetTime()
        {
            _currentTime = _currentTime.AddMinutes(1);
            return _currentTime;
        }

    }
}
