using System.Runtime.CompilerServices;
using Universe.Carotene.Union.Attributes;

namespace Union.UnitTests;

public class MixedDataEventTests
{
    [Test]
    public void MatchUsesSharedStorageForStructsAndFieldsForReferences()
    {
        var large = MixedDataEvent.LargePayload(new LargePayload(1, 2, 3, 4));
        var instrument = MixedDataEvent.Instrument(new Instrument("BTC-USDT"));

        var largeResult = large.Match(
            (in _) => "small",
            (in value) => value.Fourth.ToString(),
            (_) => "instrument",
            eventStatus: (_) => "status"
        );
        var instrumentResult = instrument.Match(
            (in _) => "small",
            (in _) => "large",
            (value) => value.Symbol,
            eventStatus: (_) => "status"
        );

        Assert.Multiple(() =>
        {
            Assert.That(large.Tag, Is.EqualTo(MixedDataEvent.Kind.LargePayload));
            Assert.That(largeResult, Is.EqualTo("4"));
            Assert.That(instrument.Tag, Is.EqualTo(MixedDataEvent.Kind.Instrument));
            Assert.That(instrumentResult, Is.EqualTo("BTC-USDT"));
            Assert.That(instrumentResult, Is.EqualTo("BTC-USDT"));
            Assert.That(Unsafe.SizeOf<LargePayload>(), Is.EqualTo(32));
        });
    }
}

[Union<SmallPayload>]
[Union<LargePayload>]
[Union<Instrument>]
[Union<EventStatus>]
public readonly partial struct MixedDataEvent { }

public readonly struct SmallPayload(int value)
{
    public int Value { get; } = value;
}

public readonly struct LargePayload(long first, long second, long third, long fourth)
{
    public long First { get; } = first;
    public long Second { get; } = second;
    public long Third { get; } = third;
    public long Fourth { get; } = fourth;
}

public sealed class Instrument(string symbol)
{
    public string Symbol { get; } = symbol;
}

public sealed class EventStatus
{
    public StatusKind Status { get; set; }

    public enum StatusKind
    {
        Pending,
        Completed,
        Failed
    }    
}

