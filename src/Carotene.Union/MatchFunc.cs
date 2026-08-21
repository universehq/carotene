namespace Universe.Carotene.Union;

public delegate TResult MatchFunc<TValue, out TResult>(in TValue value)
    where TValue : struct, allows ref struct;

public delegate void MatchFunc<TValue>(in TValue value)
    where TValue : struct, allows ref struct;
