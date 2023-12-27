
using System.Linq;
using System.Collections.Generic;

using StructuralizeSharp.DataMatch;

namespace StructuralizeSharpTests.DataMatcherTests;

[TestFixture]
public class MatcherTests {

    public record NumberData(int Number) : IData;

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
}
