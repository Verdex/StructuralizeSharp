
using System.Collections.Generic;

namespace StructuralizeSharp.DataMatch;

public interface IMatcher<T> {
    IEnumerable<(string, T)> Match(IPattern pattern, T data);
}


public class Matcher : IMatcher {
    

}