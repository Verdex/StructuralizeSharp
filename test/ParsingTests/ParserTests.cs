
using StructuralizeSharp.Parsing;

namespace StructuralizeSharpTests.ParsingTests;

[TestFixture]
public class ParserTests {

    record X() : IParser<int>  {
        public ParseResult<int> Parse(IInput input)  {
            return new Success<int>(4);
        }

        public IParser<T> Select<T>(Func<int, T> t) {
           return new MapParser<int, T>(this, t);
        }
    };

    [Test]
    public void blarg() {
        var w = from x in new X() 
                from y in new X()
                select (x, y);
    }

}