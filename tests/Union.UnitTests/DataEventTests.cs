using System.Runtime.CompilerServices;
using Universe.Carotene.Union.Attributes;

namespace Union.UnitTests;

public class DataEventTests
{
    [SetUp]
    public void Setup() { }

    [Test]
    public void Test_DataEventCreate()
    {
        DataEvent dataEvent = new Data()
        {
            Address = "123 Main St",
            Age = 30,
            Name = "John Doe",
        };
        var result = dataEvent.Match(
            (data) => $"It's {data.Name}",
            (response) => "It's `Response`",
            (price) => ""
        );

        Assert.That(result, Is.EqualTo("It's John Doe"));
    }

    [Test]
    public void Test_DataEventCopy()
    {
        DataEvent dataEvent = DataEvent.Data(
            new()
            {
                Address = "123 Main St",
                Age = 30,
                Name = "John Doe",
            }
        );
        var result = dataEvent.Match(DataEvent.Data, DataEvent.Response, DataEvent.Price);

        Assert.That(result, Is.EqualTo(dataEvent));
    }

    [Test]
    public void Test_ThrowInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(
            () =>
            {
                DataEvent dataEvent = new();
                dataEvent.Match(data: (_) => { }, response: (_) => { }, price: (_) => { });
            },
            message: "Cannot match an empty DataEvent."
        );
    }

    [Test]
    public void Test_StructLayoutSize()
    {
        Assert.That(Unsafe.SizeOf<Response>(), Is.EqualTo(24));
    }
}

[Union<Data>]
[Union<Response>]
[Union<Price>]
public readonly partial struct DataEvent { }

public readonly struct Data
{
    public int Age { get; init; } // 4 + padding 4
    public string Name { get; init; } // 8
    public string Address { get; init; } // 8
}

public readonly struct Price
{
    public decimal Value { get; init; }
}

public readonly struct Response
{
    public int Age { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime ClosedAt { get; init; }
}
