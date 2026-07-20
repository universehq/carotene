using BenchmarkDotNet.Attributes;
using Universe.Carotene.Collections;

namespace DequeBenchmark;

[MemoryDiagnoser]
public class DequeBenchmark
{
    private const int Size = 10_000;

    private Queue<int> _queue = null!;
    private SimpleDeque<int> _deque = null!;

    [GlobalSetup]
    public void Setup()
    {
        _queue = new Queue<int>(Size);
        _deque = new SimpleDeque<int>(Size);
    }

    [Benchmark(Baseline = true)]
    public int Queue_EnqueueDequeue()
    {
        for (int i = 0; i < Size; i++)
        {
            _queue.Enqueue(i);
        }

        int sum = 0;

        while (_queue.TryDequeue(out int value))
        {
            sum += value;
        }

        return sum;
    }

    [Benchmark]
    public int SimpleDeque_PushBackPopFront()
    {
        for (int i = 0; i < Size; i++)
        {
            _deque.PushBack(i);
        }

        int sum = 0;

        while (_deque.TryPopFront(out int value))
        {
            sum += value;
        }

        return sum;
    }

    [Benchmark]
    public int SimpleDeque_MixedFrontBack()
    {
        for (int i = 0; i < Size / 2; i++)
        {
            _deque.PushBack(i);
            _deque.PushFront(i);
        }

        int sum = 0;

        while (_deque.TryPopFront(out int value))
        {
            sum += value;
        }

        return sum;
    }
}
