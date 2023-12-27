
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
            (Exact e, _) when e.Data.Equals(data) => Success(),
            (List l, IListData listData) => MatchList(l.Patterns, listData.ToList()),
            _ => Failure(), 
        };
    }

    private IEnumerable<IEnumerable<(string, IData)>> MatchList(IPattern[] listPattern, IReadOnlyList<IData> listData) {
        if(listPattern.Length != listData.Count) {
            yield break;
        }
        foreach(var (pattern, data) in listPattern.Zip(listData)) {
            foreach( var result in Match(pattern, data) ) {
                yield return result;
            }
        }
    }

    private static IEnumerable<IEnumerable<(string, IData)>> Failure() => Array.Empty<IEnumerable<(string, IData)>>();
    private static IEnumerable<IEnumerable<(string, IData)>> Success() => new [] { Array.Empty<(string, IData)>() };
    private static IEnumerable<IEnumerable<(string, IData)>> Success(string name, IData data) 
        => new [] { new [] { (name, data) } };
}