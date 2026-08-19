using System.Runtime.CompilerServices;
using Universe.Carotene.Union;

namespace Union.UnitTests;

#pragma warning disable IDE0049,IDE0001
public readonly partial struct TestUnion
{
    private readonly global::Union.UnitTests.A _a;
    private readonly global::Union.UnitTests.B _b;
    private readonly global::Union.UnitTests.C _c;
    private readonly global::System.String? _string;

    private TestUnion(Kind kind, in global::Union.UnitTests.A value)
    {
        _a = value;
        _b = default;
        _c = default;
        _string = default;
        Tag = kind;
    }

    private TestUnion(Kind kind, in global::Union.UnitTests.B value)
    {
        _a = default;
        _b = value;
        _c = default;
        _string = default;
        Tag = kind;
    }

    private TestUnion(Kind kind, in global::Union.UnitTests.C value)
    {
        _a = default;
        _b = default;
        _c = value;
        _string = default;
        Tag = kind;
    }

    private TestUnion(Kind kind, global::System.String value)
    {
        _a = default;
        _b = default;
        _c = default;
        _string = value;
        Tag = kind;
    }

    public static TestUnion @A(global::Union.UnitTests.A value) => new(Kind.A, value);

    public static TestUnion @B(global::Union.UnitTests.B value) => new(Kind.B, value);

    public static TestUnion @C(global::Union.UnitTests.C value) => new(Kind.C, value);

    public static TestUnion @String(global::System.String value) => new(Kind.String, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Match(
        MatchFunc<global::Union.UnitTests.A> @a,
        MatchFunc<global::Union.UnitTests.B> @b,
        MatchFunc<global::Union.UnitTests.C> @c,
        Action<global::System.String> @string
    )
    {
        switch (Tag)
        {
            case Kind.None:
                throw new InvalidOperationException();

            case Kind.A:
                @a(in _a);
                break;

            case Kind.B:
                @b(in _b);
                break;

            case Kind.C:
                @c(in _c);
                break;

            case Kind.String:
                @string(_string!);
                break;
        }
    }

    public Kind Tag { get; }

    public enum Kind
    {
        None,
        @A,
        @B,
        @C,
        @String,
    }
}

public struct A { }

public struct B { }

public struct C { }
#pragma warning restore IDE0049,IDE0001
