using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Trade.Tests
{
    [TestClass()]
    public class EDSystemManagerTests
    {
        private int TotalSystemCount;

        private void DataSetup()
        {
            var exeRoot = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var dataRoot = Directory.GetParent(Directory.GetParent(exeRoot).ToString()).ToString();
            var MainData = Path.Combine(dataRoot, "Data", "systems.csv");
            var RecentData = Path.Combine(dataRoot, "Data", "systems_recent.csv");

            TotalSystemCount = File.ReadLines(MainData).Count() - 1;

            EDSystemManager.Instance.DataPath = MainData;
            EDSystemManager.Instance.RecentDataPath = RecentData;
        }

        private void UpdateDataSetup()
        {
            DataSetup();
            EDSystemManager.Instance.Update(false);
        }

        [TestMethod()]
        public void UpdateTest()
        {
            Assert.Inconclusive();
        }

        [TestMethod()]
        public void DownloadDataTest()
        {
            Assert.Inconclusive();
        }

        [TestMethod()]
        public void LoadFromFileTest()
        {
            DataSetup();
            EDSystemManager.Instance.Update(false);

            Assert.AreEqual(TotalSystemCount, EDSystemManager.Instance.Systems.Count());
        }

        [TestMethod()]
        public void FindTest()
        {
            UpdateDataSetup();

            var found = EDSystemManager.Instance.Find("olgrea");
            Assert.AreEqual("Olgrea", found.name);
            Assert.AreEqual(14960, found.id);

            found = EDSystemManager.Instance.Find("Col 285 Sector GM-V d2-108");
            Assert.AreEqual("Col 285 Sector GM-V d2-108", found.name);
            Assert.AreEqual(69658, found.id);
        }

        [TestMethod()]
        public void FindInRangeTest()
        {
            Assert.Inconclusive();
        }
    }
}