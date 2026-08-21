using System.Runtime.CompilerServices;
using DotNext;

namespace Universe.Carotene.Collections;

public sealed class SimpleDeque<T>(int capacity)
    where T : notnull
{
    private readonly T[] _buffer = GC.AllocateUninitializedArray<T>(capacity);

    private int _head;

    public int Capacity => _buffer.Length;
    public int Count { get; private set; }

    public bool IsFull => Count == _buffer.Length;
    public bool IsEmpty => Count == 0;

    private int Wrap(int index)
    {
        int cap = _buffer.Length;
        return index >= cap ? index - cap : index;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool PushBack(T value)
    {
        if (IsFull)
        {
            return false;
        }

        _buffer[Wrap(_head + Count)] = value;
        Count++;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool PushFront(T value)
    {
        if (IsFull)
        {
            return false;
        }

        _head--;
        if (_head < 0)
        {
            _head += _buffer.Length;
        }

        _buffer[_head] = value;
        Count++;
        return true;
    }

    public Optional<T> PopFront()
    {
        if (IsEmpty)
        {
            return Optional<T>.None;
        }

        var value = _buffer[_head];
        _head = Wrap(_head + 1);
        Count--;
        return value;
    }

    public Optional<T> PopBack()
    {
        if (IsEmpty)
        {
            return Optional<T>.None;
        }

        int tail = Wrap(_head + Count - 1);
        var value = _buffer[tail];
        Count--;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPopFront(out T value)
    {
        if (IsEmpty)
        {
            value = default!;
            return false;
        }

        value = _buffer[_head];
        _head = Wrap(_head + 1);
        Count--;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPopBack(out T value)
    {
        if (IsEmpty)
        {
            value = default!;
            return false;
        }

        int tail = Wrap(_head + Count - 1);
        value = _buffer[tail];
        Count--;
        return true;
    }

    public ref T Front
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _buffer[_head];
    }

    public ref T Back
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _buffer[Wrap(_head + Count - 1)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T At(int index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Count);

        return ref _buffer[Wrap(_head + index)];
    }

    public Span<T> FirstSpan
    {
        get
        {
            int len = Math.Min(Count, _buffer.Length - _head);
            return _buffer.AsSpan(_head, len);
        }
    }

    public Span<T> SecondSpan
    {
        get
        {
            int first = Math.Min(Count, _buffer.Length - _head);
            return _buffer.AsSpan(0, Count - first);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            FirstSpan.Clear();
            SecondSpan.Clear();
        }

        _head = 0;
        Count = 0;
    }
}
