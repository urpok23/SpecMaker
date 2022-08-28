using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace SpecMakerTests;

[TestClass]
public class TestIsQuantityPatternSatisfied
{
    const string pattern = @"\s\d+\s+шт.$";

    [TestMethod]
    public void TestMatch()
    {

        var helper = new Tuple<string, bool>[] { 
            new ("316/316LCL.150RF - 1 шт.", true),
            new ("316/316L CL.150RF -  шт.", false),
            new ("F 316/L CL.150RF - 1шт.", false),
            new (" 316/316L C50RF -  1шт. ", false),
            new ("F 316L CL.150RF - 91 шт.", true),
            new ("316/316LCL.150Fmjh87 шт.", false),
            new ("  91 шт.", true),
            new (" 91 шт. ", false),
        };

        string[] testCases = helper.Select(t => t.Item1).ToArray();
        bool[] expected = helper.Select(t => t.Item2).ToArray();
        
        bool[] res = new bool[testCases.Length];

        foreach ( (int i, string testCase) in testCases.Select((s, i) => (i, s)) )
        {
            res[i] = SpecMaker.Utilities.IsQuantityPatternSatisfied(testCase, pattern);
        }
        
        foreach (var r in res) Console.WriteLine(r);
        Assert.IsTrue(expected.SequenceEqual(res));
    }

}

