using System.Runtime.CompilerServices;
using Universe.Carotene.Union;

namespace Union.UnitTests;

public sealed partial class Message : IUnion
{
    private readonly Text? _text;
    private readonly Image? _image;
    private readonly Audio? _audio;

    private Message(Text value)
    {
        Tag = Kind.Text;
        _text = value;
    }

    private Message(Image value)
    {
        Tag = Kind.Image;
        _image = value;
    }

    private Message(Audio value)
    {
        Tag = Kind.Audio;
        _audio = value;
    }

    public static Message Text(Text value) => new(value);

    public static Message Image(Image value) => new(value);

    public static Message Audio(Audio value) => new(value);

    public Kind Tag { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Match(Action<Text> text, Action<Image> image, Action<Audio> audio)
    {
        switch (Tag)
        {
            case Kind.None:
                throw new InvalidOperationException("Cannot match an empty DataEvent.");
            case Kind.Text:
                text(_text!);
                break;
            case Kind.Image:
                image(_image!);
                break;
            case Kind.Audio:
                audio(_audio!);
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Match<T>(Func<Text, T> text, Func<Image, T> image, Func<Audio, T> audio)
    {
        return Tag switch
        {
            Kind.None => throw new InvalidOperationException("Cannot match an empty DataEvent."),
            Kind.Text => text(_text!),
            Kind.Image => image(_image!),
            Kind.Audio => audio(_audio!),
            _ => throw new InvalidOperationException($"Unknown DataEvent.Kind: {Tag}."),
        };
    }

    public interface IMessage { }

    public enum Kind
    {
        None,
        Text,
        Image,
        Audio,
    }
}

public partial record Text();

public partial record Text : Message.IMessage;

public partial record Image();

public partial record Image : Message.IMessage;

public partial record Audio();

public partial record Audio : Message.IMessage;
