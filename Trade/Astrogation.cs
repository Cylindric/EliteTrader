using EliteTrader.Models;
using System;


namespace Trade
{
    public class Astrogation
    {
        public static float Distance(EDSystem from, EDSystem to)
        {
            return (float)Math.Sqrt(Math.Pow(to.x - from.x, 2) + Math.Pow(to.y - from.y, 2) + Math.Pow(to.z - from.z, 2));
        }

        public static float ManhattanDistance(EDSystem from, EDSystem to)
        {
            return Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y) + Math.Abs(from.z - to.z);
        }
    }
}
