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
            Assert.AreEqual(8, route.Count());
        }

        [TestMethod()]
        public void RouteTestNoSystemsInRange()
        {
            // You can't get from Olgrea to Sheela Na Gig with 5LY jumps, it is 6.597303LY
            DataSetup();
            var j = new RouteFinder();
            j.JumpRange = 5;
            var route = j.Route(EDSystemManager.Instance.Find("Olgrea"), EDSystemManager.Instance.Find("Sheela Na Gig"));
            Assert.AreEqual(0, route.Count());
        }

        [TestMethod()]
        public void RouteTest2()
        {
            DataSetup();
            var j = new RouteFinder();
            j.JumpRange = 30.0F;
            var route = j.Route(EDSystemManager.Instance.Find("HIP 41181"), EDSystemManager.Instance.Find("Vamm"));
            Assert.AreEqual(6, route.Count());
        }

        [TestMethod]
        public void RouteCacheTest()
        {
            var sw = new Stopwatch();

            DataSetup();
            RouteFinder.ClearCache();

            var rf1 = new RouteFinder();
            rf1.JumpRange = 30.0F;
            sw.Start();
            rf1.Route(EDSystemManager.Instance.Find("Tocorii"), EDSystemManager.Instance.Find("Te Kaha"));
            sw.Stop();
            var uncachedTime = sw.ElapsedTicks;
            Assert.AreEqual(false, rf1.LastResultWasFromCache);

            var rf2 = new RouteFinder();
            rf2.JumpRange = 30.0F;
            sw.Reset();
            sw.Start();
            rf2.Route(EDSystemManager.Instance.Find("Tocorii"), EDSystemManager.Instance.Find("Te Kaha"));
            sw.Stop();
            var cachedTime = sw.ElapsedTicks;

            Assert.AreEqual(true, rf2.LastResultWasFromCache);
        }

        [TestMethod]
        public void RouteCacheTestDifferentRanges()
        {
            var sw = new Stopwatch();

            DataSetup();
            RouteFinder.ClearCache();

            var rf1 = new RouteFinder();
            rf1.JumpRange = 30.0F;
            sw.Start();
            rf1.Route(EDSystemManager.Instance.Find("HIP 41181"), EDSystemManager.Instance.Find("Vamm"));
            sw.Stop();
            var uncachedTime = sw.ElapsedTicks;
            Assert.AreEqual(false, rf1.LastResultWasFromCache);

            var rf2 = new RouteFinder();
            rf2.JumpRange = 40F;
            sw.Reset();
            sw.Start();
            rf2.Route(EDSystemManager.Instance.Find("HIP 41181"), EDSystemManager.Instance.Find("Vamm"));
            sw.Stop();
            var cachedTime = sw.ElapsedTicks;

            Assert.AreEqual(false, rf2.LastResultWasFromCache);
        }
    }
}