
using System.Collections.Generic;

namespace StructuralizeSharp.DataMatch;

public interface IMatcher {
    IEnumerable<IEnumerable<(string, IData)>> Match(IPattern pattern, IData data);
}


public class Matcher : IMatcher {
    public IEnumerable<IEnumerable<(string, IData)>> Match(IPattern pattern, IData data) {
        return (pattern, data) switch {
            (Capture c, _) => new [] { new [] { (c.Name, data) } },
            _ => throw new NotImplementedException(),
        };
    }
}