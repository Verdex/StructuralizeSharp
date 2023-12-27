
using System.Linq;
using System.Collections.Generic;

using StructuralizeSharp.DataMatch;
using PList = StructuralizeSharp.DataMatch.List;

namespace StructuralizeSharpTests.DataMatcherTests;

[TestFixture]
public class MatcherTests {

    public record NumberData(int Number) : IData;
    public record StringData(string Str) : IData;

    [Test]
    public void ShouldCaptureData() {
        var output = new Matcher().Match(new Capture("x"), new NumberData(5)).ToList();

        Assert.That(output.Count, Is.EqualTo(1));
        Assert.That(output[0].Count, Is.EqualTo(1));

        var match = output[0].ToDictionary();

        Assert.That(match["x"], Is.EqualTo(new NumberData(5)));
    }

    [Test]
    public void ShouldMatchWildCards() {
        var output = new Matcher().Match(new Wild(), new NumberData(5)).ToList();

        Assert.That(output.Count, Is.EqualTo(1));
        Assert.That(output[0].Count, Is.EqualTo(0));
    }

    private static readonly object[] MatchExactTestData = new object[] {
        new object[] { new NumberData(5), new NumberData(5), true},
        new object[] { new NumberData(4), new NumberData(5), false},
        new object[] { new NumberData(4), new StringData("4"), false},
    };

    [TestCaseSource(nameof(MatchExactTestData))]
    public void ShouldMatchExact(IData pattern, IData data, bool successful) {
        var output = new Matcher().Match(new Exact(pattern), data).ToList();

        if (successful) {
            Assert.That(output.Count, Is.EqualTo(1));
            Assert.That(output[0].Count, Is.EqualTo(0));
        }
        else {
            Assert.That(output.Count, Is.EqualTo(0));
        }
    }

    private static readonly object[] MatchListTestData = new object[] {
        new object[] { new PList(new Exact(new NumberData(5))), new ListData(new NumberData(5)), true},
        new object[] { new PList(new Exact(new NumberData(5)), new Exact(new NumberData(4))), 
                       new ListData(new NumberData(5), new NumberData(4)), 
                       true
                     },
    };

    [TestCaseSource(nameof(MatchListTestData))]
    public void ShouldMatchList(PList pattern, IData data, bool successful) {
        var output = new Matcher().Match(pattern, data).ToList();

        if (successful) {
            Assert.That(output.Count, Is.EqualTo(1));
            Assert.That(output[0].Count, Is.EqualTo(0));
        }
        else {
            Assert.That(output.Count, Is.EqualTo(0));
        }
    }
}
