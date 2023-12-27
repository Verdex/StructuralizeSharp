namespace StructuralizeSharp.DataMatch;


public interface IData<TAtom> {

}

public interface IPattern<TAtom> { }

public record Capture(string Name) : IPattern;