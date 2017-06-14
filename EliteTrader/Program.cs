using Trade;
using EliteTrader.Models;

namespace EliteTrader
{
    class Program
    {
        private static RaptorDB.RaptorDB rdb;

        static void Main(string[] args)
        {
            rdb = RaptorDB.RaptorDB.Open("data");
            RaptorDB.Global.RequirePrimaryView = false;

            EDSystem.rdb = rdb;

            EDSystem.Update();
            // EDStation.Update();
            
            var j = new RouteFinder();
            j.JumpRange = 30.0F;
            j.Route("Ringardha", "Te Kaha");

            rdb.Shutdown();
        }
    }
}
