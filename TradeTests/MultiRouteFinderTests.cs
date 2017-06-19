using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using EliteTrader.Models;

namespace Trade.Tests
{
    [TestClass()]
    public class MultiRouteFinderTests
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
            var mr = new MultiRouteFinder();
            mr.JumpRange = 30.0F;

            var start = EDSystemManager.Instance.Find("Olgrea");
            var routes = new List<Queue<EDSystem>>();

            var routeA = new Queue<EDSystem>();
            routeA.Enqueue(EDSystemManager.Instance.Find("Te Kaha"));
            routeA.Enqueue(EDSystemManager.Instance.Find("Cao Junga"));
            routes.Add(routeA);

            var routeB = new Queue<EDSystem>();
            routeB.Enqueue(EDSystemManager.Instance.Find("Carnsan"));
            routeB.Enqueue(EDSystemManager.Instance.Find("HIP 13179"));
            routes.Add(routeB);

            var finalRoute = mr.Route(start, routes);
            Assert.AreEqual(6, finalRoute.Count());

            int i = 0;
            EDSystem prev = null;
            RouteFinder r = new RouteFinder();
            r.JumpRange = 30;

            while (finalRoute.Count() > 0)
            {
                var dest = finalRoute.Dequeue();
                Console.Write($"{i} {dest.name}");
                if (prev != null)
                {
                    var distance = Astrogation.Distance(prev, dest);
                    var jumps = r.Route(prev, dest).Count();
                    Console.Write($" ({distance:n1} LY, {jumps:n0} jumps)");
                }
                Console.WriteLine();

                prev = dest;
                i++;

            }
        }
    }
}