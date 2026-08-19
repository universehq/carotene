using Microsoft.CodeAnalysis;

internal sealed class TypeSizeCalculator(int pointerSize)
{
    private readonly Dictionary<ITypeSymbol, TypeLayout> _cache = new(
        SymbolEqualityComparer.Default
    );

    private readonly int _pointerSize = pointerSize;

    public int GetApproximateTypeSize(ITypeSymbol typeSymbol) => GetLayout(typeSymbol).Size;

    private TypeLayout GetLayout(ITypeSymbol typeSymbol)
    {
        if (_cache.TryGetValue(typeSymbol, out var cached))
        {
            return cached;
        }

        TypeLayout layout = CalculateLayout(typeSymbol);

        _cache.Add(typeSymbol, layout);
        return layout;
    }

    private TypeLayout CalculateLayout(ITypeSymbol typeSymbol)
    {
        if (typeSymbol.IsReferenceType)
        {
            return new TypeLayout(_pointerSize, _pointerSize);
        }

        int primitiveSize = GetPrimitiveSize(typeSymbol);
        if (primitiveSize > 0)
        {
            return new TypeLayout(primitiveSize, primitiveSize);
        }

        if (typeSymbol.TypeKind == TypeKind.Enum && typeSymbol is INamedTypeSymbol enumType)
        {
            return GetLayout(enumType.EnumUnderlyingType!);
        }

        if (typeSymbol.TypeKind != TypeKind.Struct)
        {
            return new TypeLayout(_pointerSize, _pointerSize);
        }

        int size = 0;
        int alignment = 1;

        foreach (var field in typeSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            if (field.IsStatic)
            {
                continue;
            }

            TypeLayout fieldLayout = GetLayout(field.Type);

            size = AlignUp(size, fieldLayout.Alignment);
            size += fieldLayout.Size;

            if (fieldLayout.Alignment > alignment)
            {
                alignment = fieldLayout.Alignment;
            }
        }

        size = AlignUp(size, alignment);

        return new TypeLayout(size, alignment);
    }

    private int GetPrimitiveSize(ITypeSymbol typeSymbol)
    {
        return typeSymbol.SpecialType switch
        {
            SpecialType.System_Boolean => 1,
            SpecialType.System_Byte => 1,
            SpecialType.System_SByte => 1,

            SpecialType.System_Char => 2,
            SpecialType.System_Int16 => 2,
            SpecialType.System_UInt16 => 2,

            SpecialType.System_Int32 => 4,
            SpecialType.System_UInt32 => 4,
            SpecialType.System_Single => 4,

            SpecialType.System_Int64 => 8,
            SpecialType.System_UInt64 => 8,
            SpecialType.System_Double => 8,

            SpecialType.System_IntPtr => _pointerSize,
            SpecialType.System_UIntPtr => _pointerSize,

            SpecialType.System_Decimal => 16,

            _ => 0,
        };
    }

    private static int AlignUp(int value, int alignment)
    {
        if (alignment <= 1)
        {
            return value;
        }

        return (value + alignment - 1) / alignment * alignment;
    }

    private readonly record struct TypeLayout(int Size, int Alignment)
    {
        public int Size { get; } = Size;
        public int Alignment { get; } = Alignment;
    }
}
