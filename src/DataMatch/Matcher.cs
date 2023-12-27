
using System.Collections.Generic;

namespace StructuralizeSharp.DataMatch;

public interface IMatcher {
    IEnumerable<IEnumerable<(string, IData)>> Match(IPattern pattern, IData data);
}


public class Matcher : IMatcher {

    public IEnumerable<IEnumerable<(string, IData)>> Match(IPattern pattern, IData data) {
        return (pattern, data) switch {
            (Wild, _) => Success(),
            (Capture c, _) => Success(c.Name, data),
            _ => throw new NotImplementedException(),
        };
    }

    private static IEnumerable<IEnumerable<(string, IData)>> Success() => new [] { Array.Empty<(string, IData)>() };
    private static IEnumerable<IEnumerable<(string, IData)>> Success(string name, IData data) 
        => new [] { new [] { (name, data) } };
}