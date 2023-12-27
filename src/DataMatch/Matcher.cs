
using System.Collections.Generic;

namespace StructuralizeSharp.DataMatch;

public interface IMatcher {
    IEnumerable<(string, IData)> Match(IPattern pattern, IData data);
}


public class Matcher : IMatcher {
    

}