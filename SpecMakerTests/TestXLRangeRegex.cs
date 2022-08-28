using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecMakerTests
{
    [TestClass]
    public class TestXLRangeRegex
    {
        private const string xlRangePattern = @"^[A-Z]+[1-9]\d*$";

        [TestMethod]
        [DataRow("", false)]
        [DataRow("A", false)]
        [DataRow(" A", false)]
        [DataRow("A ", false)]
        [DataRow("AA", false)]
        [DataRow("A0", false)]
        [DataRow("AA0", false)]
        [DataRow("A01", false)]
        [DataRow("AA01", false)]
        [DataRow("A1A", false)]
        [DataRow("1A", false)]
        [DataRow("a1", false)]
        [DataRow("A1", true)]
        [DataRow("AA1", true)]
        [DataRow("A10", true)]
        [DataRow("AA10", true)]
        public void Test(string xlRange, bool expected)
        {
            bool res = SpecMaker.Utilities.IsXLRangePatternSatisfied(xlRange, xlRangePattern);
            Assert.AreEqual(expected, res);
        }
    }
}
