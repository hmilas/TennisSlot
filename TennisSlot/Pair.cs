using System;
using System.Collections.Generic;
using System.Linq;

namespace TennisSlot
{
    public class Pair
    {
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
    }

    public static class PairExtensions
    {
        public static List<Pair> ShufflePairs(this List<Pair> pairs)
        {
            var rnd = new Random();
            var pairsCount = pairs.Count();

            for (int i = 0; i < pairsCount - 1; i++)
            {
                var position = rnd.Next(0, pairsCount - i);
                var rndElem = pairs.ElementAt(position);

                pairs.RemoveAt(position);
                pairs.Insert(pairsCount - i - 1, rndElem);
            }

            return pairs;
        }
    }
}
