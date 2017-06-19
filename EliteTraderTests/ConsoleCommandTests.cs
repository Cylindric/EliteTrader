using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace EliteTrader.Tests
{
    [TestClass()]
    public class ConsoleCommandTests
    {
        [TestMethod()]
        public void ConsoleCommandBasicTest()
        {
            var cmd = new ConsoleCommand("foo bar baz");
            Assert.AreEqual("foo", cmd.Name);
            Assert.AreEqual(2, cmd.Arguments.Count());
        }

        [TestMethod()]
        public void ConsoleCommandQuotedTest()
        {
            var cmd = new ConsoleCommand("foo \"bar baz\"");
            Assert.AreEqual("foo", cmd.Name);
            Assert.AreEqual(1, cmd.Arguments.Count());
            Assert.AreEqual("bar baz", cmd.Arguments.First().Value);
        }

        [TestMethod()]
        public void ConsoleCommandNamedParameters()
        {
            var cmd = new ConsoleCommand("foo bar=baz something=other");
            Assert.AreEqual("foo", cmd.Name);
            Assert.AreEqual(2, cmd.Arguments.Count());

            Assert.AreEqual("bar", cmd.Arguments.First().Key);
            Assert.AreEqual("baz", cmd.Arguments.First().Value);
            Assert.AreEqual("something", cmd.Arguments.Skip(1).First().Key);
            Assert.AreEqual("other", cmd.Arguments.Skip(1).First().Value);
        }

        [TestMethod()]
        public void ConsoleCommandQuotedNamedParameters()
        {
            var cmd = new ConsoleCommand("foo bar=\"baz something\" other");
            Assert.AreEqual("foo", cmd.Name);
            Assert.AreEqual(2, cmd.Arguments.Count());

            Assert.AreEqual("bar", cmd.Arguments.First().Key);
            Assert.AreEqual("baz something", cmd.Arguments.First().Value);
            Assert.AreEqual("other", cmd.Arguments.Skip(1).First().Value);
        }

        [TestMethod()]
        public void ConsoleCommandSimilarParameters()
        {
            var cmd = new ConsoleCommand("foo bar=\"baz\" bar=\"boo\"");
            Assert.AreEqual("foo", cmd.Name);
            Assert.AreEqual(2, cmd.Arguments.Count());

            Assert.AreEqual("bar", cmd.Arguments.First().Key);
            Assert.AreEqual("baz", cmd.Arguments.First().Value);

            Assert.AreEqual("bar", cmd.Arguments.Skip(1).First().Key);
            Assert.AreEqual("boo", cmd.Arguments.Skip(1).First().Value);
        }
    }
}