using static Union.UnitTests.DataEvent;

namespace Union.UnitTests;

public class PlaygroudTests
{
    [SetUp]
    public void Setup() { }

    [Test]
    public void Test_DataEventCreate()
    {
        DataEvent dataEvent = Data(new() { });
        var result = dataEvent.Match(
            (in response) => "It's `Response`",
            (in data) => "It's `Data`"
        );

        Assert.That(result, Is.EqualTo("It's `Data`"));
    }

    [Test]
    public void Test_DataEventCopy()
    {
        DataEvent dataEvent = Data(new() { });
        var reuslt = dataEvent.Match((in response) => Response(response), (in data) => Data(data));

        Assert.That(reuslt, Is.EqualTo(dataEvent));
    }

    [Test]
    public void Test_DataEventIType()
    {
        DataEvent dataEvent = Data(new() { });
        var reuslt = dataEvent.Match(
#pragma warning disable CS0183,IDE0001
            (in response) => response is DataEvent.IDataEvent,
#pragma warning restore CS0183,IDE0001

#pragma warning disable CS0183,IDE0001
            (in data) => data is DataEvent.IDataEvent
#pragma warning restore CS0183,IDE0001
        );

        Assert.That(reuslt, Is.True);
    }

    [Test]
    public void Test_ThrowInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(
            () =>
            {
                DataEvent dataEvent = new();
                dataEvent.Match((in _) => { }, (in _) => { });
            },
            message: "Cannot match an empty DataEvent."
        );
    }
}
