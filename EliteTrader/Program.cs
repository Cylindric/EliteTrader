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
            EDSystemManager.Instance.Update();
            // EDStation.Update();
            
            var j = new RouteFinder();
            j.JumpRange = 30.0F;
            var route=j.Route("Ringardha", "Te Kaha");

            Console.WriteLine($"Path found, Ringardha to Te Kaha in {route.Count - 1} jumps.");
            var i = 0;
            foreach (var n in route)
            {
                Console.WriteLine($"{i:n0} {n.name}");
                i++;
            }


        }
    }
}
