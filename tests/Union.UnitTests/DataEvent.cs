using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Universe.Carotene.Union;

namespace Union.UnitTests;

public readonly struct DataEvent : IUnion
{
    private readonly Storage _storage;

    private DataEvent(Kind kind, Storage storage)
    {
        Tag = kind;
        _storage = storage;
    }

    public static DataEvent Response(Response value) => new(Kind.Response, Storage.Create(value));

    public static DataEvent Data(Data value) => new(Kind.Data, Storage.Create(value));

    public Kind Tag { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Match(MatchFunc<Response> response, MatchFunc<Data> data)
    {
        switch (Tag)
        {
            case Kind.None:
                throw new InvalidOperationException("Cannot match an empty DataEvent.");
            case Kind.Response:
                response(in Get<Response>());
                break;
            case Kind.Data:
                data(in Get<Data>());
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Match<T>(MatchFunc<Response, T> response, MatchFunc<Data, T> data)
    {
        return Tag switch
        {
            Kind.None => throw new InvalidOperationException("Cannot match an empty DataEvent."),
            Kind.Response => response(in Get<Response>()),
            Kind.Data => data(in Get<Data>()),
            _ => throw new InvalidOperationException($"Unknown DataEvent.Kind: {Tag}."),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref readonly T Get<T>()
        where T : struct => ref Unsafe.As<Storage, T>(ref Unsafe.AsRef(in _storage));

    public enum Kind
    {
        None,
        Response,
        Data,
    }

    public interface IDataEvent { }

    [StructLayout(LayoutKind.Explicit, Size = 64)]
    private readonly struct Storage
    {
        [FieldOffset(0)]
        private readonly ulong _data;

        public static Storage Create<T>(T value)
            where T : struct
        {
            Storage storage = default;
            Unsafe.As<Storage, T>(ref storage) = value;
            return storage;
        }
    }
}

public readonly partial struct Response { }

public partial struct Response : DataEvent.IDataEvent { }

public readonly partial struct Data { }

public partial struct Data : DataEvent.IDataEvent { }
