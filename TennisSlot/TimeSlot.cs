using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace TennisSlot
{
    public class TimeSlot
    {
        public DateTime PlayTime { get; set; }
        public Pair Pair { get; set; }
        public string Result { get; set; }

        public TimeSlot() { }

        public TimeSlot(DateTime playTime)
        {
            PlayTime = playTime;
        }

        public static readonly List<DateTime> ExcludeDateTimes = new List<DateTime>
        {
            new DateTime(2016, 11, 1),
            new DateTime(2016, 12, 25),
            new DateTime(2016, 12, 26),
            new DateTime(2017, 1, 1),
            new DateTime(2017, 1, 6),
            new DateTime(2017, 4, 16),
            new DateTime(2017, 4, 17)
        };

        public static List<TimeSlot> GenerateTimeSlots(int numOfMonths, DateTime startDate, List<DayOfWeek> daysOfWeek, List<double> hoursOfPlay)
        {
            var timeSlots = new List<TimeSlot>();
            int numOfTimeSlots = numOfMonths * 4 * daysOfWeek.Count();

            var slotDate = startDate.AddDays(-1);
            while (true)
            {
                slotDate = slotDate.AddDays(1);

                if (ExcludeDateTimes.Contains(slotDate) || !daysOfWeek.Contains(slotDate.DayOfWeek))
                    continue;

                foreach (var hourOfPlay in hoursOfPlay)
                    timeSlots.Add(new TimeSlot(slotDate.AddHours(hourOfPlay)));

                if (numOfTimeSlots-- < 0)
                    break;
            }

            return timeSlots;
        }

        public static List<TimeSlot> LoadFromExcel(string timeSlotsFileLocation)
        {
            var timeSlots = new List<TimeSlot>();

            var localFile = string.Empty;
            var fileInfo = GetExcelFile(timeSlotsFileLocation, out localFile);

            using (var package = new ExcelPackage(fileInfo))
            {
                var i = 2;
                while (true)
                {
                    var playersScheduleWorksheet = package.Workbook.Worksheets[1];
                    DateTime playTime;
                    DateTime.TryParseExact((string)playersScheduleWorksheet.Cells[i, 1].Value, "dd.MM.yyyy. HH:mm",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out playTime);

                    var timeSlot = new TimeSlot
                    {
                        PlayTime = playTime,
                        Pair = new Pair
                        {
                            Player1 = new Player { Name = (string)playersScheduleWorksheet.Cells[i, 2].Value },
                            Player2 = new Player { Name = (string)playersScheduleWorksheet.Cells[i, 3].Value }
                        }
                    };

                    if (timeSlot.PlayTime != default(DateTime))
                    {
                        timeSlots.Add(timeSlot);
                    }

                    i++;

                    if (timeSlot.PlayTime == default(DateTime) &&
                        string.IsNullOrWhiteSpace(timeSlot.Pair.Player1.Name) &&
                        string.IsNullOrWhiteSpace(timeSlot.Pair.Player2.Name))
                    {
                        break;
                    }
                }
            }

            if (localFile != timeSlotsFileLocation)
                File.Delete(localFile);

            return timeSlots;
        }

        public static FileInfo GetExcelFile(string timeSlotsFileLocation, out string localFile)
        {
            localFile = Guid.NewGuid().ToString("N") + ".xlsx";
            if (timeSlotsFileLocation.StartsWith("http"))
            {
                using (WebClient client = new WebClient())
                {
                    client.Credentials = CredentialCache.DefaultNetworkCredentials;
                    client.DownloadFile(timeSlotsFileLocation, localFile);
                }
            }
            else
            {
                localFile = timeSlotsFileLocation;
            }

            return new FileInfo(localFile);
        }
    }

    public static class TimeSlotExtensions
    {
        public static void ExportToExcel(this List<TimeSlot> timeSlots)
        {
            var folder = "..\\..\\GeneratedTimeSlots";
            var generatedFileName = string.Format("Raspored-{0}.xlsx", DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss"));
            var filePath = Path.Combine(folder, generatedFileName);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var file = File.Create(filePath);

            using (var package = new ExcelPackage(file))
            {
                ExcelWorksheet playersScheduleWorksheet = package.Workbook.Worksheets.Add("Raspored");

                var i = 1;
                playersScheduleWorksheet.Row(1).Style.Font.Size = 13;
                playersScheduleWorksheet.Row(1).Style.Font.Bold = true;

                playersScheduleWorksheet.Cells[i, 1].Value = "Termin";
                playersScheduleWorksheet.Cells[i, 2].Value = "Igrač";
                playersScheduleWorksheet.Cells[i, 3].Value = "Igrač";
                playersScheduleWorksheet.Cells[i, 4].Value = "Rezultat";

                i++;

                foreach (var timeSlot in timeSlots)
                {
                    playersScheduleWorksheet.Cells[i, 1].Value = timeSlot.PlayTime.ToString("dd.MM.yyyy. HH:mm");
                    playersScheduleWorksheet.Cells[i, 2].Value = timeSlot.Pair.Player1.ToString();
                    playersScheduleWorksheet.Cells[i, 3].Value = timeSlot.Pair.Player2.ToString();

                    i++;
                }

                playersScheduleWorksheet.Workbook.Worksheets.Add("Igrači");
                var playerListWorksheet = playersScheduleWorksheet.Workbook.Worksheets[2];

                i = 1;
                playerListWorksheet.Row(1).Style.Font.Size = 13;
                playerListWorksheet.Row(1).Style.Font.Bold = true;

                playerListWorksheet.Cells[i, 1].Value = "Ime";
                playerListWorksheet.Cells[i, 2].Value = "Email";
                i++;

                foreach (var player in Player.PlayerList)
                {
                    playerListWorksheet.Cells[i, 1].Value = player.ToString();
                    playerListWorksheet.Cells[i, 2].Value = player.Email;

                    i++;
                }

                playersScheduleWorksheet.Cells.AutoFitColumns();
                playerListWorksheet.Cells.AutoFitColumns();

                package.Save();
            }
        }

        public static List<TimeSlot> SchedulePlayers(this List<TimeSlot> timeSlots)
        {
            var playerPairCombinations = Player.PlayerList.GeneratePlayerPairCombinations();

            Func<List<Pair>> getPairList = () => new List<Pair>(playerPairCombinations).ShufflePairs();
            var pairList = getPairList();
            var playerPairCount = Player.PlayerList.Count() / 2;
            var currentScheduleSector = 0;

            foreach (var timeSlot in timeSlots)
            {
                if (!pairList.Any())
                    pairList = getPairList();

                var pair = pairList.First();
                var i = 0;
                var currentSectorAddedPlayers = new List<Player>();
                var items = timeSlots.Where(x => x.Pair != null)
                                     .Skip(currentScheduleSector * playerPairCount)
                                     .Take(playerPairCount)
                                     .Select(x => new { x.Pair.Player1, x.Pair.Player2 });

                foreach (var item in items)
                {
                    if (!currentSectorAddedPlayers.Contains(item.Player1)) currentSectorAddedPlayers.Add(item.Player1);
                    if (!currentSectorAddedPlayers.Contains(item.Player2)) currentSectorAddedPlayers.Add(item.Player2);
                }
                var pairListCount = pairList.Count();

                while (true)
                {
                    if (currentSectorAddedPlayers.Contains(pair.Player1) ||
                        currentSectorAddedPlayers.Contains(pair.Player2))
                    {
                        if (i + 1 >= pairListCount)
                        {
                            currentScheduleSector++;
                            break;
                        }
                        pair = pairList.Skip(i++).Take(1).First();
                        continue;
                    }

                    break;
                }

                timeSlot.Pair = pair;
                pairList.Remove(pair);
            }

            return timeSlots;
        }

        public static void AlertPlayersForTomorrow(this List<TimeSlot> timeSlots, List<Player> playerList)
        {
            timeSlots.AlertPlayers(playerList, DateTime.Now.AddDays(1).Date, 
                "Tenis liga - podsjetnik o sutrašnjem terminu", "Sutra u <u>{0}h</u> imate termin protiv igrača: <u>{1}</u>  <br /><br /> Pozdrav");
        }

        public static void AlertPlayersForToday(this List<TimeSlot> timeSlots, List<Player> playerList)
        {
            timeSlots.AlertPlayers(playerList, DateTime.Now.Date, 
                "Tenis liga - podsjetnik o današnjem terminu", "Danas u <u>{0}h</u> imate termin protiv igrača: <u>{1}</u> <br /><br /> Pozdrav");
        }

        private static void AlertPlayers(this List<TimeSlot> timeSlots, List<Player> playerList, 
            DateTime alertDate, string subject, string bodyMessage)
        {
            bodyMessage = "<div style='font-family: Trebuchet MS; font-size: 14px;'>" + bodyMessage + "</div>";
            var alertTimeSlots = timeSlots.Where(x => x.PlayTime.Date == alertDate);

            Action<string, TimeSlot, Player> sendMail = (playerName, timeSlot, opponent) =>
            {
                var playerMail = playerList.GetEmail(playerName);
                if (playerMail != null)
                {
                    Email.Send(new MailMessage("tenis.liga.omega@gmail.com", playerMail)
                    {
                        Subject = subject,
                        Body = string.Format(bodyMessage, timeSlot.PlayTime.ToString("HH:mm"), opponent),
                        IsBodyHtml = true
                    });
                }
            };

            foreach (var alertTimeSlot in alertTimeSlots)
            {
                sendMail(alertTimeSlot.Pair.Player1.Name, alertTimeSlot, alertTimeSlot.Pair.Player2);
                sendMail(alertTimeSlot.Pair.Player2.Name, alertTimeSlot, alertTimeSlot.Pair.Player1);
            }
        }
    }
}
