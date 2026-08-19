namespace Universe.Carotene.Union;

public ref struct Storage()
{
    public unsafe void* ValueRef;

    // Copy once here
    public static Storage Create<T>(T value)
    {
        unsafe
        {
            Storage storage = default;

#pragma warning disable CS8500
            storage.ValueRef = &value;
#pragma warning restore CS8500

            return storage;
        }
    }
}
