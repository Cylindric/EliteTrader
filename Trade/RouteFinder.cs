using EliteTrader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trade
{
    public class RouteFinder
    {
        public float JumpRange { get; set; }

        public void Route(string fromSystem, string toSystem)
        {
            var from = EDSystem.Find(fromSystem);
            var to = EDSystem.Find(toSystem);

        }

    }
}
