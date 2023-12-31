
using StructuralizeSharp.Parsing;

namespace StructuralizeSharpTests.ParsingTests;

[TestFixture]
public class ParserTests {

    class X : IParser<int>  {
        private readonly IParser<int> _p = 
               from a in ParserExt.Any()
               where a == 'x'
               select 5;

        public IParseResult<int> Parse(Input input) => _p.Parse(input);
    };

    [Test]
    public void InputShouldRestore() {
        var input = new Input("input");
        var rp0 = input.RestorePoint();

        input.TryNext(out var x);
        Assert.That( x, Is.EqualTo('i'));
        input.TryNext(out var y);
        Assert.That( y, Is.EqualTo('n'));
        
        var rp = input.RestorePoint();

        input.TryNext(out var z);
        Assert.That( z, Is.EqualTo('p'));

        input.TryNext(out var w);
        Assert.That( w, Is.EqualTo('u'));

        input.Restore(rp);

        input.TryNext(out var a);
        Assert.That( a, Is.EqualTo('p') );

        input.Restore(rp0);
        input.TryNext(out var b);
        Assert.That( b, Is.EqualTo('i') );
    }

    [Test]
    public void InputShouldTryNext() {
        var input = new Input("in");

        var r1 = input.TryNext(out var a1);
        Assert.That(r1, Is.True);
        Assert.That(a1, Is.EqualTo('i'));
        
        var r2 = input.TryNext(out var a2);
        Assert.That(r2, Is.True);
        Assert.That(a2, Is.EqualTo('n'));

        var r3 = input.TryNext(out var _);
        Assert.That(r3, Is.False);
    }

    [Test]
    public void AnyShouldParseAny() {
        var input = new Input("x");
        var p = ParserExt.Any();

        var output = p.Parse(input).Unwrap();
        Assert.That(output, Is.EqualTo('x'));
    }

    [Test]
    public void AnyShouldNotParseExhaustedInput() {
        var input = new Input("");
        var p = ParserExt.Any();

        var output = p.Parse(input);

        Assert.That( output.IsError(), Is.True );
    }

    [Test]
    public void SelectShouldMapResult() {
        var input = new Input("x");
        var p = new X();

        var output = p.Select(x => x + 1).Parse(input).Unwrap();

        Assert.That(output, Is.EqualTo(6));
    }

    [Test]
    public void SelectTwiceShouldMapResult() {
        var input = new Input("x");
        var p = new X();

        var output = p.Select(x => x + 1).Select(x => x + 1).Parse(input).Unwrap();

        Assert.That(output, Is.EqualTo(7));
    }

    [Test]
    public void FatalSelectShouldIndicateFatalAndLeaveInputAtFailurePoint() {
        var input = new Input("_xy");
        var p = from a in new X()
                from b in new X().Fatal()
                select 5;

        input.TryNext(out var _);
        var output = p.Select(x => x + 1).Parse(input);

        Assert.That(output.IsFatal(), Is.True);
        Assert.That(input.Index, Is.EqualTo(2));
    }

    [Test]
    public void ErrorSelectShouldIndicateErrorAndBackupRestorePoint() {
        var input = new Input("_xy");
        var p = from a in new X()
                from b in new X()
                select 5;

        input.TryNext(out var _);
        var output = p.Select(x => x + 1).Parse(input);

        Assert.That(output.IsError(), Is.True);
        Assert.That(input.Index, Is.EqualTo(1));
    }

    [Test]
    public void AlternateShouldParseFirstOption() {
        var input = new Input("xx");

        var p1 = from a in new X()
                 from b in new X()
                 select 5;

        var p2 = ParserExt.Any().Select(_ => 0);

        var output = p1.Alt(p2).Parse(input).Unwrap();

        Assert.That(output, Is.EqualTo(5));
    }

    [Test]
    public void AlternateShouldParseSecondOption() {
        var input = new Input("z");

        var p1 = from a in new X()
                 from b in new X()
                 select 5;

        var p2 = from a in ParserExt.Any()
                 where a == 'z'
                 select 0;

        var output = p1.Alt(p2).Parse(input).Unwrap();

        Assert.That(output, Is.EqualTo(0));
    }

    [Test]
    public void EndShouldParseEndInput() {
        var input = new Input("");

        var output = ParserExt.End().Parse(input);

        Assert.That(output.IsSuccess(), Is.True);
    }

    [Test]
    public void EndShouldNotParseNotEndInput() {
        var input = new Input("x");

        var output = ParserExt.End().Parse(input);

        Assert.That(output.IsError(), Is.True);

        var anyOutput = ParserExt.Any().Parse(input).Unwrap();

        Assert.That(anyOutput, Is.EqualTo('x'));
    }

    [Test]
    public void ZeroOrMoreShouldParseMultipleItems() {
        var input = new Input("xyxyzz");

        var p = from x in ParserExt.Any()
                from y in ParserExt.Any()
                where (x, y) is ('x', 'y')
                select 0;

        var output = p.ZeroOrMore().Parse(input).Unwrap().ToArray();

        Assert.That(output.Length, Is.EqualTo(2));
    }

    [Test]
    public void ZeroOrMoreShouldParseMultipleItemsWhenLastParseExceedsInput() {
        var input = new Input("xyxyz");

        var p = from x in ParserExt.Any()
                from y in ParserExt.Any()
                where (x, y) is ('x', 'y')
                select 0;

        var output = p.ZeroOrMore().Parse(input).Unwrap().ToArray();

        Assert.That(output.Length, Is.EqualTo(2));

        var z = ParserExt.Any().Parse(input).Unwrap();

        Assert.That(z, Is.EqualTo('z'));
    }

    [Test]
    public void ZeroOrMoreShouldParseMultipleItemsWhenLastParseHitsEnd() {
        var input = new Input("xyxy");

        var p = from x in ParserExt.Any()
                from y in ParserExt.Any()
                where (x, y) is ('x', 'y')
                select 0;

        var output = p.ZeroOrMore().Parse(input).Unwrap().ToArray();

        Assert.That(output.Length, Is.EqualTo(2));

        var e = ParserExt.End().Parse(input);

        Assert.That(e.IsSuccess(), Is.True);
    }
}