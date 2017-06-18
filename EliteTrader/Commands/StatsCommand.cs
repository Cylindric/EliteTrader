using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trade;

namespace EliteTrader.Commands
{
    class StatsCommand : ICommand
    {
        public int Execute()
        {
            var edsm = EDSystemManager.Instance;

            var xMin = float.MaxValue;
            var yMin = float.MaxValue;
            var zMin = float.MaxValue;
            var xMax = float.MinValue;
            var yMax = float.MinValue;
            var zMax = float.MinValue;

            Models.EDSystem xMinSys = new Models.EDSystem();
            Models.EDSystem yMinSys = new Models.EDSystem();
            Models.EDSystem zMinSys = new Models.EDSystem();
            Models.EDSystem xMaxSys = new Models.EDSystem();
            Models.EDSystem yMaxSys = new Models.EDSystem();
            Models.EDSystem zMaxSys = new Models.EDSystem();

            foreach (var sys in edsm.Systems)
            {
                if (sys.Value.x < xMin) { xMin = sys.Value.x; xMinSys = sys.Value; }
                if (sys.Value.y < yMin) { yMin = sys.Value.y; yMinSys = sys.Value; }
                if (sys.Value.z < zMin) { zMin = sys.Value.z; zMinSys = sys.Value; }
                if (sys.Value.x > xMax) { xMax = sys.Value.x; xMaxSys = sys.Value; }
                if (sys.Value.y > yMax) { yMax = sys.Value.y; yMaxSys = sys.Value; }
                if (sys.Value.z > zMax) { zMax = sys.Value.z; zMaxSys = sys.Value; }
            }

            Console.WriteLine("");
            Console.WriteLine($"Galaxy minimum values: [{xMin}, {yMin}, {zMin}].");
            Console.WriteLine($"Galaxy maximum values: [{xMax}, {yMax}, {zMax}].");
            Console.WriteLine($"Galaxy minimum systems: [{xMinSys.name}, {yMinSys.name}, {zMinSys.name}].");
            Console.WriteLine($"Galaxy maximum systems: [{xMaxSys.name}, {yMaxSys.name}, {zMaxSys.name}].");
            Console.WriteLine($"Galaxy size: [{xMax-xMin}, {yMax-yMin}, {zMax-zMin}].");
            Console.WriteLine($"Galaxy contains {edsm.Systems.Count:n0} known systems.");
            Console.WriteLine("");
            return 0;
        }
    }
}
