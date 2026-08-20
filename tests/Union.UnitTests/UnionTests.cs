using System.Runtime.CompilerServices;
using Universe.Carotene.Union;

namespace Union.UnitTests;

public class UnionTests
{
    [Test]
    public void TestSizeOfUnionStruct()
    {
        Assert.That(Unsafe.SizeOf<TestUnion>(), Is.EqualTo(24));
    }
}

#pragma warning disable IDE0049,IDE0001
public readonly partial struct TestUnion
{
    private readonly global::Union.UnitTests.A _a;
    private readonly global::Union.UnitTests.B _b;
    private readonly global::Union.UnitTests.C _c;
    private readonly global::System.String? _string;
    private readonly global::Union.UnitTests.Contract? _contract;

    private TestUnion(Kind kind, in global::Union.UnitTests.A value)
    {
        _a = value;
        _b = default;
        _c = default;
        _string = default;
        _contract = default;
        Tag = kind;
    }

    private TestUnion(Kind kind, in global::Union.UnitTests.B value)
    {
        _a = default;
        _b = value;
        _c = default;
        _string = default;
        _contract = default;
        Tag = kind;
    }

    private TestUnion(Kind kind, in global::Union.UnitTests.C value)
    {
        _a = default;
        _b = default;
        _c = value;
        _string = default;
        _contract = default;
        Tag = kind;
    }

    private TestUnion(Kind kind, global::System.String value)
    {
        _a = default;
        _b = default;
        _c = default;
        _string = value;
        _contract = default;
        Tag = kind;
    }

    private TestUnion(Kind kind, global::Union.UnitTests.Contract value)
    {
        _a = default;
        _b = default;
        _c = default;
        _string = default;
        _contract = value;
        Tag = kind;
    }

    public static TestUnion @A(global::Union.UnitTests.A value) => new(Kind.A, value);

    public static TestUnion @B(global::Union.UnitTests.B value) => new(Kind.B, value);

    public static TestUnion @C(global::Union.UnitTests.C value) => new(Kind.C, value);

    public static TestUnion @String(global::System.String value) => new(Kind.String, value);

    public static TestUnion @Contract(global::Union.UnitTests.Contract value) =>
        new(Kind.String, value);

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

public class Contract { }
#pragma warning restore IDE0049,IDE0001
