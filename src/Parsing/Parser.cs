
namespace StructuralizeSharp.Parsing; 


public record ParseResult<T>();
public record Fatal<T>() : ParseResult<T>;
public record Error<T>() : ParseResult<T>;
public record Success<T>(T Value) : ParseResult<T>;

public interface IParser<T> { 
    ParseResult<T> Parse(IEnumerator<char> input);
}

public class MapParser<T, S> : IParser<S> {
    private readonly IParser<T> _parser;
    private readonly Func<T, S> _t;
    public MapParser(IParser<T> parser, Func<T, S> t) {
        _parser = parser;
        _t = t;
    }

    public ParseResult<S> Parse(IEnumerator<char> input) =>
    // TODO need a restore point
        _parser.Parse(input) switch {
            Fatal<T> _ => new Fatal<S>(),
            Error<T> _ => new Error<S>(), // TODO probably need to chain some stuff from the old one here
            Success<T> s => new Success<S>(_t(s.Value)),
            _ => throw new Exception(),
        };
}

public class FlatMapParser<T, S, R> : IParser<R> {
    private readonly IParser<T> _parser;
    private readonly Func<T, IParser<S>> _next;
    private readonly Func<T, S, R> _final;
    public FlatMapParser(IParser<T> parser, Func<T, IParser<S>> next, Func<T, S, R> final) {
        _parser = parser;
        _next = next;
        _final = final;
    }

    public ParseResult<R> Parse(IEnumerator<char> input) {
        // TODO need restore point
        var r = _parser.Parse(input);
        if ( r is not Success<T> s ) {
            // TODO
            throw new Exception();
        }

        var p = _next(s.Value);

        return p.Select(s2 => _final(s.Value, s2)).Parse(input);   //_final(s.Value, s2.Value );
    }
}

public static class ParserExt {
    public static IParser<S> Select<T, S>(this IParser<T> parser, Func<T, S> t) => new MapParser<T, S>(parser, t);
    public static IParser<R> SelectMany<T, S, R>(this IParser<T> parser, Func<T, IParser<S>> next, Func<T, S, R> final)
        => new FlatMapParser<T, S, R>(parser, next, final);
}