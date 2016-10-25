using System;
using System.Collections.Generic;
using TennisSlot;

namespace Scheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            var numOfMonths = 6;
            var startDate = new DateTime(2016, 10, 24);
            var daysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Thursday };
            var hoursOfPlay = new List<double> { 18, 19 };

            TimeSlot.GenerateTimeSlots(numOfMonths, startDate, daysOfWeek, hoursOfPlay)
                .SchedulePlayers()
                .ExportToExcel();
        }
    }
}
