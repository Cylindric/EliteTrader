using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliteTrader.Models;
using System.IO;
using System.Diagnostics;

namespace Trade.Tests
{
    [TestClass()]
    public class RouteFinderTests
    {
        private void DataSetup()
        {
            var exeRoot = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var dataRoot = Directory.GetParent(Directory.GetParent(exeRoot).ToString()).ToString();
            var MainData = Path.Combine(dataRoot, "Data", "systems.csv");
            var RecentData = Path.Combine(dataRoot, "Data", "systems_recent.csv");

            EDSystemManager.Instance.DataPath = MainData;
            EDSystemManager.Instance.RecentDataPath = RecentData;
            EDSystemManager.Instance.Update(false);
        }

        [TestMethod()]
        public void RouteTest()
        {
            DataSetup();
            var j = new RouteFinder();
            j.JumpRange = 30.0F;
            var route = j.Route(EDSystemManager.Instance.Find("Olgrea"), EDSystemManager.Instance.Find("Te Kaha"));

            Console.WriteLine($"Path found, Olgrea to Te Kaha in {route.Count - 1} jumps.");
            var i = 0;
            foreach (var n in route)
            {
                Console.WriteLine($"{i:n0} {n.name}");
                i++;
            }

            /*
             * Route from http://www.miggy.org/games/elite-dangerous/routes/:
             * 
                0	Olgrea	                        0.00	0.00	0.00
                1	Bei Dou Sector FW-W b1-2 (M)	23.42	23.42	23.42
                2	Tirada (M)	                    29.38	52.81	47.96
                3	Liabefa (M)	                    29.46	82.26	75.56
                4	HIP 13179	                    29.06	111.32	104.11
                5	Lyncis Sector XP-P b5-6 (M)	    27.89	139.21	130.61
                6	Col 285 Sector ES-H b11-3 (M)	28.51	167.72	158.46
                7	Col 285 Sector NY-Q c5-18 (K)	29.60	197.33	185.55
                8	Te Kaha (M)	                    29.84	227.17	214.32

            * Route from here:
                0 Olgrea
                1 LP 63-267
                2 Tirada
                3 Liabefa
                4 HIP 13179
                5 Lyncis Sector XP-P b5-6
                6 Col 285 Sector ES-H b11-3
                7 Col 285 Sector NY-Q c5-18
                8 Te Kah
*/
            Assert.AreEqual(9, route.Count());
        }

        [TestMethod]
        public void RouteCacheTest()
        {
            var sw = new Stopwatch();

            DataSetup();
            var j = new RouteFinder();
            j.JumpRange = 30.0F;

            sw.Start();
            j.Route(EDSystemManager.Instance.Find("Olgrea"), EDSystemManager.Instance.Find("Te Kaha"));
            sw.Stop();
            var uncachedTime = sw.ElapsedTicks;

            Assert.AreEqual(false, j.LastResultWasFromCache);

            sw.Reset();
            sw.Start();
            j.Route(EDSystemManager.Instance.Find("Olgrea"), EDSystemManager.Instance.Find("Te Kaha"));
            sw.Stop();
            var cachedTime = sw.ElapsedTicks;

            Assert.AreEqual(true, j.LastResultWasFromCache);

            Assert.IsTrue(cachedTime / uncachedTime <= 0.005, "Cached route should be much faster than an uncached route.");
        }
    }
}