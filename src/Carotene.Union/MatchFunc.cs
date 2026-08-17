namespace Universe.Carotene.Union;

public delegate TResult MatchFunc<TValue, out TResult>(in TValue value);

public delegate void MatchFunc<TValue>(in TValue value);
