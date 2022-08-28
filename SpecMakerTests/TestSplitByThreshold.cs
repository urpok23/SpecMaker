using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace SpecMakerTests
{
    [TestClass]
    public class TestSplitByThreshold
    {
        [TestMethod]
        public void OneWordLongString()
        {
            string s = "однодлинноепримечаниедлятеста";
            string[] expectedArr = new[] { "однодлинноепримечаниедлятеста" };
            var resArr = SpecMaker.ColumnSplitter.SplitByThreshold(s, 17);

            Assert.IsTrue(expectedArr.SequenceEqual(resArr));
        }

        [TestMethod]
        public void SimpleString()
        {
            string s = "это примечание для приварного клапана";
            string[] expectedArr = new[] { "это примечание", "для приварного", "клапана" };
            var resArr = SpecMaker.ColumnSplitter.SplitByThreshold(s, 17);

            Assert.IsTrue(expectedArr.SequenceEqual(resArr));
        }
        [TestMethod]
        [DataRow(null)]
        [DataRow("   ")]
        public void NullString(string s)
        {
            string[] expectedArr = new[] { string.Empty };
            var resArr = SpecMaker.ColumnSplitter.SplitByThreshold(s, 17);

            Assert.IsTrue(expectedArr.SequenceEqual(resArr));
        }

    }
}