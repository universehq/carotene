namespace Universe.Carotene.Union.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public sealed class UnionAttribute<T> : Attribute
    where T : allows ref struct { }
