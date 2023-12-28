using System.Collections.Generic;

namespace StructuralizeSharp.DataMatch;

public interface IData { }
public interface IListData : IData { 
    IReadOnlyList<IData> ToList();
}

public record ListData(params IData[] datas) : IListData {
    public IReadOnlyList<IData> ToList() => datas.ToList().AsReadOnly();
}

public interface IPattern { }

public record Capture(string Name) : IPattern;
public record Wild() : IPattern;
public record Exact(IData Data) : IPattern;
public record List(params IPattern[] Patterns) : IPattern;