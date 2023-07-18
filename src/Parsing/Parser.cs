
namespace StructuralizeSharp.Parsing; 


public record ParseResult<T>();
public record Fatal<T>() : ParseResult<T>;
public record Error<T>() : ParseResult<T>;
public record Success<T>(T Value) : ParseResult<T>;

internal static class A {
    public static T B<T>(Func<T> f) => f();
}

public class Input {
    private readonly string _input;
    private int _index;
    public Input(string input, int index = 0) {
        _input = input;
        _index = index;
    }

    public bool TryNext(out char value) {
        if ( _index < _input.Length ) {
            value = _input[_index];
            _index += 1;
            return true;
        }
        else {
            value = '\0';
            return false;
        }
    }

    public Input RestorePoint() => new Input(_input, _index);
    public void Restore(Input rp) {
        _index = rp._index;
    }
}

public interface IParser<T> { 
    ParseResult<T> Parse(Input input);
}

public class MapParser<T, S> : IParser<S> {
    private readonly IParser<T> _parser;
    private readonly Func<T, S> _t;
    public MapParser(IParser<T> parser, Func<T, S> t) {
        _parser = parser;
        _t = t;
    }

    public ParseResult<S> Parse(Input input) {
        var rp = input.RestorePoint();
        return _parser.Parse(input) switch {
            Fatal<T> _ => new Fatal<S>(),// TODO probably need to chain some stuff from the old one here
            Error<T> _ => A.B( () => {
                input.Restore(rp);
                return new Error<S>();
            } ),
            Success<T> s => new Success<S>(_t(s.Value)),
            _ => throw new Exception(),
        };
    }
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

    public ParseResult<R> Parse(Input input) {
        var rp = input.RestorePoint();
        var r = _parser.Parse(input);

        if ( r is Error<T> _ ) {
            input.Restore(rp);
            return new Error<R>();
        }

        if ( r is Fatal<T> _ ) {
            return new Fatal<R>();
        }

        if ( r is not Success<T> s ) {
            throw new Exception();
        }

        var p = _next(s.Value);
        var r2 = p.Parse(input);

        if ( r2 is Error<S> _ ) {
            input.Restore(rp);
            return new Error<R>();
        }

        if ( r2 is Fatal<S> _ ) {
            return new Fatal<R>();
        }

        if ( r2 is not Success<S> s2 ) {
            throw new Exception();
        }

        return new Success<R>(_final(s.Value, s2.Value));
    }
}

public class WhereParser<T> : IParser<T> {
    private readonly IParser<T> _parser;
    private readonly Func<T, bool> _pred;
    public WhereParser(IParser<T> parser, Func<T, bool> pred) {
        _parser = parser;
        _pred = pred;
    }

    public ParseResult<T> Parse(Input input) {
        var rp = input.RestorePoint();
        return _parser.Parse(input) switch {
            Error<T> _ => A.B( () => {
                input.Restore(rp);
                return new Error<T>();
            }), // TODO probably need to chain some stuff from the old one here
            Success<T> s when _pred(s.Value) => new Success<T>(s.Value),
            Success<T> _ => A.B( () => {
                input.Restore(rp);
                return new Error<T>();
            }),  // TODO probably needs to indicate the type of failure
            Fatal<T> _ => new Fatal<T>(),
            _ => throw new Exception(),
        };
    }
}

public class FatalParser<T> : IParser<T> {
    private readonly IParser<T> _parser;
    public FatalParser(IParser<T> parser) {
        _parser = parser;
    }

    public ParseResult<T> Parse(Input input) =>
        _parser.Parse(input) switch {
            Fatal<T> _ => new Fatal<T>(),
            Error<T> _ => new Fatal<T>(), 
            Success<T> s => s, 
            _ => throw new Exception(),
        };
}

public class AlternateParser<T> : IParser<T> {
    private readonly IParser<T> _parserA;
    private readonly IParser<T> _parserB;
    public AlternateParser(IParser<T> parserA, IParser<T> parserB) {
        _parserA = parserA;
        _parserB = parserB;
    }

    public ParseResult<T> Parse(Input input) {
        var rp = input.RestorePoint();

        var resultA = _parserA.Parse(input);    

        if ( resultA is Fatal<T> f ) {
            return f;
        }

        if ( resultA is Success<T> s ) {
            return s;
        }

        if ( resultA is Error<T> _ ) {
            input.Restore(rp);
            return _parserB.Parse(input) switch {
                Fatal<T> f2 => f2,
                Success<T> s2 => s2,
                Error<T> e => A.B( () => {
                    input.Restore(rp);
                    return e;
                }),
                _ => throw new Exception(),
            };
        }

        throw new Exception();
    }
}

public static class ParserExt {
    public static IParser<S> Select<T, S>(this IParser<T> parser, Func<T, S> t) => new MapParser<T, S>(parser, t);
    public static IParser<R> SelectMany<T, S, R>(this IParser<T> parser, Func<T, IParser<S>> next, Func<T, S, R> final)
        => new FlatMapParser<T, S, R>(parser, next, final);
    public static IParser<T> Where<T>(this IParser<T> parser, Func<T, bool> pred) => new WhereParser<T>(parser, pred);
    public static IParser<T> Fatal<T>(this IParser<T> parser) => new FatalParser<T>(parser);
    public static IParser<T> Alt<T>(this IParser<T> parserA, IParser<T> parserB) => new AlternateParser<T>(parserA, parserB);

    // TODO end
    // TODO zero or more
    // TODO Maybe
}