using Universe.Carotene.Collections;

namespace Collections.UnitTests;

public class SimpleDequeTest
{
    [SetUp]
    public void Setup() { }

    [Test]
    public void Obtain_Test()
    {
        var capacity = 1_024;
        SimpleDeque<int> simpleDeque = new(capacity);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(simpleDeque.Capacity, Is.EqualTo(capacity));
            Assert.That(simpleDeque.Count, Is.EqualTo(0));
        }
    }

    [Test]
    public void Push_Test()
    {
        var capacity = 3;
        SimpleDeque<int> simpleDeque = new(capacity);

        _ = simpleDeque.PushBack(0); // 2
        _ = simpleDeque.PushBack(1); // 3
        _ = simpleDeque.PushBack(2); // 4
        _ = simpleDeque.TryPopBack(out _);
        _ = simpleDeque.PushBack(3);
        _ = simpleDeque.PushBack(4);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(simpleDeque.Front, Is.EqualTo(0));
            Assert.That(simpleDeque.Back, Is.EqualTo(3));
            Assert.That(simpleDeque.Count, Is.EqualTo(3));
        }
    }

    [Test]
    public void Modifty_Test()
    {
        var capacity = 2;
        SimpleDeque<int> simpleDeque = new(capacity);

        _ = simpleDeque.PushBack(0); // 2
        _ = simpleDeque.PushBack(1); // 3

        _ = simpleDeque.TryPopBack(out _);
        _ = simpleDeque.PushBack(2);

        _ = simpleDeque.Back = 100;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(simpleDeque.Front, Is.EqualTo(0));
            Assert.That(simpleDeque.Back, Is.EqualTo(100));
        }
    }

    [Test]
    public void DefaultValue_Test()
    {
        var capacity = 2;
        SimpleDeque<int> simpleDeque = new(capacity);

        Assert.That(simpleDeque.Front, Is.Default);
    }

    [Test]
    public void Buffer_Test()
    {
        bool result;
        var capacity = 2;
        SimpleDeque<int> simpleDeque = new(capacity);

        Assert.That(simpleDeque.IsEmpty, Is.True);

        _ = simpleDeque.PushBack(0);
        _ = simpleDeque.PushBack(1);

        Assert.That(simpleDeque.IsFull, Is.True);

        _ = simpleDeque.TryPopBack(out _);
        result = simpleDeque.TryPopBack(out _);

        Assert.That(result, Is.True);

        _ = simpleDeque.PushBack(1);
        var front = simpleDeque.PopFront();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(front.HasValue, Is.True);
            Assert.That(front.Value, Is.EqualTo(1));
        }
    }
}
