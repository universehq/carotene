namespace Playground.Tests;

public class TypeTests
{
    [SetUp]
    public void Setup() { }

    [Test]
    public void Test_TypeName()
    {
        Assert.That(typeof(int).Name, Is.EqualTo("Int32"));
    }
}
