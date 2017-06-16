using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliteTrader.Models;

namespace Trade.Tests
{
    [TestClass()]
    public class AstrogationTests
    {
        [TestMethod()]
        public void DistanceTest()
        {
            var s1 = new EDSystem() { x = 0, y = 0, z = 0 };
            var s2 = new EDSystem() { x = 2, y = 3, z = 6 };

            Assert.AreEqual(7, Astrogation.Distance(s1, s2));
        }

        [TestMethod()]
        public void DistanceTestOlgreaToBeiDou()
        {
            var s1 = new EDSystem() { x = -28.125F, y = 68.28125F, z = -6.375F }; // Olgrea
            var s2 = new EDSystem() { x = -49F, y = 66.3125F, z = -16.8125F }; // Bei Dou Sector FW-W b1-2

            Assert.AreEqual(Math.Round(23.42185, 4), Math.Round(Astrogation.Distance(s1, s2), 4));
        }

        [TestMethod()]
        public void ManhattanDistanceTest()
        {
            var s1 = new EDSystem() {x = 0, y = 0, z = 0};
            var s2 = new EDSystem() {x = 1, y = 2, z = 3};

            Assert.AreEqual(6, Astrogation.ManhattanDistance(s1, s2));
        }
    }
}