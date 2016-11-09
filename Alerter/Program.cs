using System.Configuration;
using TennisSlot;

namespace Alerter
{
    class Program
    {
        static void Main(string[] args)
        {
            var timeSlotsFileLocation = ConfigurationManager.AppSettings["TimeSlotsFileLocation"];
            var timeSlots = TimeSlot.LoadFromExcel(timeSlotsFileLocation);
            var playerList = Player.LoadFromExcel(timeSlotsFileLocation);

            if (args.Length > 0 && args[0] == "today")
            {
                timeSlots.AlertPlayersForToday(playerList);
            }
            else
            {
                timeSlots.AlertPlayersForTomorrow(playerList);
            }
        }
    }
}
