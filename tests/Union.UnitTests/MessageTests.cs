using Universe.Carotene.Union.Attributes;

namespace Union.UnitTests;

public class MessageTests
{
    [SetUp]
    public void Setup() { }

    [Test]
    public void Test1()
    {
        Message message = new Audio();

        var result = message.Match(
            text => "Text",
            image => "Image",
            audio => "Audio",
            @string => "string"
        );

        Assert.Multiple(() =>
        {
            Assert.That(message.Tag, Is.EqualTo(Message.Kind.Audio));
            Assert.That(result, Is.EqualTo("Audio"));
        });
    }
}

[Union<Text>]
[Union<Image>]
[Union<Audio>]
[Union<string>]
public sealed partial class Message { }

public record Text() { }

public record Image() { }

public record Audio() { }
