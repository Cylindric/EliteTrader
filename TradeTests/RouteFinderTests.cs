using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliteTrader.Models;
using System.IO;

namespace Trade.Tests
{
    [TestClass()]
    public class RouteFinderTests
    {
        [TestMethod()]
        public void RouteTest()
        {
            var exeRoot = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var data = Directory.GetParent(Directory.GetParent(exeRoot).ToString()).ToString();
            data = Path.Combine(data, "Data", "systems.csv");

            EDSystemManager.Instance.DataPath = data;
            EDSystemManager.Instance.Update();

            var j = new RouteFinder();
            j.JumpRange = 30.0F;
            var route = j.Route("Olgrea", "Te Kaha");

            Console.WriteLine($"Path found, Olgrea to Te Kaha in {route.Count - 1} jumps.");
            var i = 0;
            foreach (var n in route)
            {
                Console.WriteLine($"{i:n0} {n.name}");
                i++;
            }

            /*
                0	Olgrea	0.00	0.00	0.00
                1	Bei Dou Sector FW-W b1-2 (M)	23.42	23.42	23.42
                2	Tirada (M)	29.38	52.81	47.96
                3	Liabefa (M)	29.46	82.26	75.56
                4	HIP 13179	29.06	111.32	104.11
                5	Lyncis Sector XP-P b5-6 (M)	27.89	139.21	130.61
                6	Col 285 Sector ES-H b11-3 (M)	28.51	167.72	158.46
                7	Col 285 Sector NY-Q c5-18 (K)	29.60	197.33	185.55
                8	Te Kaha (M)	29.84	227.17	214.32
             */
            Assert.AreEqual(9, route.Count());
        }
    }
}