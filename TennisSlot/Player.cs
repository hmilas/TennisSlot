using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace TennisSlot
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }

        public override string ToString()
        {
            return Name + " " + Surname;
        }

        private static List<Player> _playerList = new List<Player>();
        public static List<Player> PlayerList
        {
            get
            {
                if(_playerList.Count() == 0)
                {
                    var playerInfos = File.ReadLines(@"..\..\players.txt");
                    var i = 1;
                    foreach(var playerInfo in playerInfos)
                    {
                        var playerInfoSplitted = playerInfo.Split(';');
                        _playerList.Add(new Player
                        {
                            Id = i++,
                            Name = playerInfoSplitted[0],
                            Surname = playerInfoSplitted[1],
                            Email = playerInfoSplitted[2]
                        });
                    }
                }

                return _playerList;
            }
        }

        public static List<Player> LoadFromExcel(string timeSlotsFileLocation)
        {
            var playerList = new List<Player>();
            var fileInfo = new FileInfo(timeSlotsFileLocation);

            using (var package = new ExcelPackage(fileInfo))
            {
                var i = 2;
                while (true)
                {
                    var playerListWorksheet = package.Workbook.Worksheets[2];
 
                    var player = new Player
                    {
                        Name = (string)playerListWorksheet.Cells[i, 1].Value,
                        Email = (string)playerListWorksheet.Cells[i, 2].Value
                    };

                    if (!string.IsNullOrWhiteSpace(player.Email))
                    {
                        playerList.Add(player);
                    }

                    i++;

                    if (string.IsNullOrWhiteSpace(player.Name) &&
                        string.IsNullOrWhiteSpace(player.Email))
                    {
                        break;
                    }
                }
            }

            return playerList;
        }
    }

    public static class PlayerExtensions
    {
        public static List<Pair> GeneratePlayerPairCombinations(this List<Player> playerList)
        {
            var pairs = new List<Pair>();
            for (var i = 0; i < playerList.Count; i++)
            {
                for (var j = i + 1; j < playerList.Count; j++)
                {
                    pairs.Add(new Pair { Player1 = playerList[i], Player2 = playerList[j] });
                }
            }

            return pairs;
        }

        public static string GetEmail(this List<Player> playerList, string playerName)
        {
            var playerMail = playerList.FirstOrDefault(x => x.Name == playerName);
            return playerMail != null ? playerMail.Email : null;
        }
    }
}
