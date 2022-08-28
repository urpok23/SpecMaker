using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace SpecMakerTests;

[TestClass]
public class TestGetProperEnding
{
    [TestMethod]
    [DataRow(0, "ий")]
    [DataRow(1, "ие")]
    [DataRow(2, "ия")]
    [DataRow(3, "ия")]
    [DataRow(4, "ия")]
    [DataRow(5, "ий")]
    [DataRow(11, "ий")]
    [DataRow(12, "ий")]
    [DataRow(21, "ие")]
    [DataRow(22, "ия")]
    [DataRow(23, "ия")]
    [DataRow(24, "ия")]
    [DataRow(25, "ий")]
    [DataRow(121, "ие")]
    [DataRow(122, "ия")]
    [DataRow(123, "ия")]
    [DataRow(124, "ия")]
    [DataRow(125, "ий")]
    public void Test(int v, string expected)
    {
        string res = SpecMaker.Utilities.GetProperEnding(v);
        Assert.IsTrue(res.SequenceEqual(expected));
    }
}