using System.Runtime.CompilerServices;

namespace Union.UnitTests;

public class UnionTests
{
    [Test]
    public void TestSizeOfUnionStruct()
    {
        Assert.That(Unsafe.SizeOf<TestUnion>(), Is.EqualTo(24));
    }
}

#pragma warning disable IDE0049
#pragma warning disable IDE0001
#pragma warning disable IDE0032
public readonly partial struct TestUnion : Universe.Carotene.Union.IUnion
{
    public Kind Tag { get; }
    private readonly global::System.Object? _value;
    public global::System.Object? Value => _value;

    private TestUnion(Kind kind, in global::Union.UnitTests.A value)
    {
        _value = value;
        Tag = kind;
    }

    private TestUnion(Kind kind, in global::Union.UnitTests.B value)
    {
        _value = value;
        Tag = kind;
    }

    private TestUnion(Kind kind, in global::Union.UnitTests.C value)
    {
        _value = value;
        Tag = kind;
    }

    private TestUnion(Kind kind, global::System.String value)
    {
        _value = value;
        Tag = kind;
    }

    private TestUnion(Kind kind, global::Union.UnitTests.Contract value)
    {
        _value = value;
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
        Action<global::Union.UnitTests.A> @a,
        Action<global::Union.UnitTests.B> @b,
        Action<global::Union.UnitTests.C> @c,
        Action<global::System.String> @string
    )
    {
        switch (Tag)
        {
            case Kind.None:
                throw new InvalidOperationException();

            case Kind.A:
                @a((A)_value!);
                break;

            case Kind.B:
                @b((B)_value!);
                break;

            case Kind.C:
                @c((C)_value!);
                break;

            case Kind.String:
                @string((global::System.String)_value!);
                break;
        }
    }

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
#pragma warning restore IDE0049
#pragma warning restore IDE0001
#pragma warning restore IDE0032
