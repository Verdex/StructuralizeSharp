
namespace StructuralizeSharp.Parsing; 


public record ParseResult<T>();
public record Fatal<T>() : ParseResult<T>;
public record Error<T>() : ParseResult<T>;
public record Success<T>(T Value) : ParseResult<T>;

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

    public ParseResult<S> Parse(Input input) =>
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

    public ParseResult<R> Parse(Input input) {
        // TODO need restore point
        var r = _parser.Parse(input);
        if ( r is not Success<T> s ) {
            // TODO
            throw new Exception();
        }

        var p = _next(s.Value);

        return p.Select(s2 => _final(s.Value, s2)).Parse(input);
    }
}

public class WhereParser<T> : IParser<T> {
    private readonly IParser<T> _parser;
    private readonly Func<T, bool> _pred;
    public WhereParser(IParser<T> parser, Func<T, bool> pred) {
        _parser = parser;
        _pred = pred;
    }

    public ParseResult<T> Parse(Input input) =>
    // TODO restore point
        _parser.Parse(input) switch {
            Fatal<T> _ => new Fatal<T>(),
            Error<T> _ => new Error<T>(), // TODO probably need to chain some stuff from the old one here
            Success<T> s when _pred(s.Value) => new Success<T>(s.Value),
            Success<T> s => new Error<T>(), // TODO probably needs to indicate the type of failure
            _ => throw new Exception(),
        };
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
    public FatalParser(IParser<T> parserA, IParser<T> parserB) {
        _parserA = parserA;
        _parserB = parserB;
    }

    public ParseResult<T> Parse(Input input) =>throw new Exception(); // TODO
}

public static class ParserExt {
    public static IParser<S> Select<T, S>(this IParser<T> parser, Func<T, S> t) => new MapParser<T, S>(parser, t);
    public static IParser<R> SelectMany<T, S, R>(this IParser<T> parser, Func<T, IParser<S>> next, Func<T, S, R> final)
        => new FlatMapParser<T, S, R>(parser, next, final);
    public static IParser<T> Where<T>(this IParser<T> parser, Func<T, bool> pred) => new WhereParser<T>(parser, pred);
    public static IParser<T> Fatal<T>(this Parser<T> parser) => new FatalParser<T>(parser);

    // TODO end
    // TODO alt
    // TODO zero or more
    // TODO Maybe
}