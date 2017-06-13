using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trade;
using EliteTrader.Models;

namespace EliteTrader
{
    class Program
    {
        static void Main(string[] args)
        {
            EDSystem.Update();
            EDStation.Update();
            
            var j = new RouteFinder();
            j.JumpRange = 30.0F;
            j.Route("Ringardha", "Te Kaha");
        }
    }
}
