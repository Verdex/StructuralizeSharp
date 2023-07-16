
using StructuralizeSharp.Parsing;

namespace StructuralizeSharpTests.ParsingTests;

[TestFixture]
public class ParserTests {

    record X() : IParser<int>  {
        public ParseResult<int> Parse(Input input)  {
            return new Success<int>(4);
        }

        public IParser<T> Select<T>(Func<int, T> t) {
           return new MapParser<int, T>(this, t);
        }
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
    public void blarg() {
        var w1 = from x in new X() 
                select x;

        var w2 = from x in new X() 
                from y in new X()
                select (x, y);

        var w3 = from x in new X() 
                from y in new X()
                from z in new X()
                select (x, y, z);

        var w4 = from x in new X() 
                from y in new X()
                where y != 0
                from z in new X()
                where z != 0
                select (x, y, z);
    }

}