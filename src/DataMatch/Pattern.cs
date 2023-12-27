namespace StructuralizeSharp.DataMatch;


public interface IData {

}

public interface IPattern { }

public record Capture(string Name) : IPattern;
public record Wild() : IPattern;
public record Exact(IData Data) : IPattern;