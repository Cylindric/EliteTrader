using Microsoft.VisualStudio.TestTools.UnitTesting;
using EliteTrader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Assert.AreEqual("bar baz", cmd.Arguments.First());
        }

        [TestMethod()]
        public void ConsoleCommandNamedParameters()
        {
            var cmd = new ConsoleCommand("foo bar=baz something=other");
            Assert.AreEqual("foo", cmd.Name);
            Assert.AreEqual(2, cmd.Arguments.Count());
            Assert.AreEqual("bar=baz", cmd.Arguments.First());
            Assert.AreEqual("something=other", cmd.Arguments.Skip(1).First());

            Assert.AreEqual(2, cmd.NamedArguments.Count());
            Assert.AreEqual("bar", cmd.NamedArguments.First().Key);
            Assert.AreEqual("baz", cmd.NamedArguments.First().Value);
            Assert.AreEqual("something", cmd.NamedArguments.Skip(1).First().Key);
            Assert.AreEqual("other", cmd.NamedArguments.Skip(1).First().Value);
        }

        [TestMethod()]
        public void ConsoleCommandQuotedNamedParameters()
        {
            var cmd = new ConsoleCommand("foo bar=\"baz something\" other");
            Assert.AreEqual("foo", cmd.Name);
            Assert.AreEqual(2, cmd.Arguments.Count());
            Assert.AreEqual("bar=baz", cmd.Arguments.First());
            Assert.AreEqual("something=other", cmd.Arguments.Skip(1).First());

            Assert.AreEqual(2, cmd.NamedArguments.Count());
            Assert.AreEqual("bar", cmd.NamedArguments.First().Key);
            Assert.AreEqual("baz", cmd.NamedArguments.First().Value);
            Assert.AreEqual("something", cmd.NamedArguments.Skip(1).First().Key);
            Assert.AreEqual("other", cmd.NamedArguments.Skip(1).First().Value);
        }
    }
}