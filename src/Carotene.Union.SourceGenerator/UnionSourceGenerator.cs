using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Universe.Carotene.Union.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class UnionSourceGenerator : IIncrementalGenerator
{
    private const string UnionAttributeName = "Universe.Carotene.Union.Attributes.UnionAttribute`1";

    private static readonly DiagnosticDescriptor UnionMustBePartial = Descriptor(
        "CAROTENEUNION001",
        "Union type must be partial",
        "Union type '{0}' must be declared partial."
    );

    private static readonly DiagnosticDescriptor MemberMustNotBeItself = Descriptor(
        "CAROTENEUNION002",
        "Union member must not be union itself",
        "Union '{0}' cannot contain itself as a member."
    );

    private static readonly DiagnosticDescriptor DuplicateMember = Descriptor(
        "CAROTENEUNION003",
        "Duplicate union member",
        "Union member '{0}' can only be added once."
    );

    private static readonly DiagnosticDescriptor UnsupportedType = Descriptor(
        "CAROTENEUNION004",
        "Unsupported union type",
        "Union targets and members must be non-static, non-generic, non-abstract top-level classes or structs."
    );

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var candidates = context.SyntaxProvider.ForAttributeWithMetadataName(
            UnionAttributeName,
            static (node, _) => node is TypeDeclarationSyntax,
            static (attributeContext, _) =>
                new Candidate(
                    (INamedTypeSymbol)attributeContext.TargetSymbol,
                    attributeContext.Attributes
                )
        );

        context.RegisterSourceOutput(
            candidates,
            static (productionContext, candidate) => Generate(productionContext, candidate)
        );
    }

    private static void Generate(SourceProductionContext context, Candidate candidate)
    {
        var valid = ValidateUnion(context, candidate.Symbol);
        var members = new List<INamedTypeSymbol>();

        foreach (var attribute in candidate.Attributes)
        {
            if (
                attribute.AttributeClass?.TypeArguments.Length != 1
                || attribute.AttributeClass.TypeArguments[0] is not INamedTypeSymbol member
            )
            {
                context.ReportDiagnostic(Diagnostic.Create(UnsupportedType, LocationOf(attribute)));
                valid = false;
                continue;
            }

            var submembers = member.GetMembers();

            if (SymbolEqualityComparer.Default.Equals(candidate.Symbol, member))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        MemberMustNotBeItself,
                        LocationOf(attribute),
                        member.ToDisplayString()
                    )
                );
                valid = false;
                continue;
            }

            if (members.Any(existing => SymbolEqualityComparer.Default.Equals(existing, member)))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DuplicateMember,
                        LocationOf(attribute),
                        member.ToDisplayString()
                    )
                );
                valid = false;
                continue;
            }

            members.Add(member);

            if (!IsSupported(member))
            {
                context.ReportDiagnostic(Diagnostic.Create(UnsupportedType, LocationOf(attribute)));
                valid = false;
            }
        }

        if (!valid || members.Count == 0)
        {
            return;
        }

        foreach (
            var name in members
                .Select(member => member.Name)
                .GroupBy(name => name, StringComparer.Ordinal)
        )
        {
            if (name.Count() > 1)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DuplicateMember,
                        candidate.Symbol.Locations.FirstOrDefault(),
                        name.Key
                    )
                );
                return;
            }
        }

        context.AddSource(
            HintName(candidate.Symbol, "Union"),
            GenerateUnion(candidate.Symbol, members)
        );
    }

    private static bool ValidateUnion(SourceProductionContext context, INamedTypeSymbol union)
    {
        if (!IsSupported(union))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(UnsupportedType, union.Locations.FirstOrDefault())
            );
            return false;
        }

        if (IsPartial(union))
        {
            return true;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                UnionMustBePartial,
                union.Locations.FirstOrDefault(),
                union.ToDisplayString()
            )
        );
        return false;
    }

    private static bool IsSupported(INamedTypeSymbol symbol)
    {
        return symbol.ContainingType is null
            && !symbol.IsStatic
            && symbol.TypeParameters.Length == 0
            && !symbol.IsAbstract
            && (symbol.TypeKind == TypeKind.Class || symbol.TypeKind == TypeKind.Struct);
    }

    private static bool IsPartial(INamedTypeSymbol symbol)
    {
        return symbol.DeclaringSyntaxReferences.Length > 0
            && symbol.DeclaringSyntaxReferences.All(reference =>
                reference.GetSyntax() is TypeDeclarationSyntax declaration
                && declaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword))
            );
    }

    private static Location LocationOf(AttributeData attribute)
    {
        return attribute.ApplicationSyntaxReference is null
            ? Location.None
            : attribute.ApplicationSyntaxReference.GetSyntax().GetLocation();
    }

    private static string GenerateUnion(
        INamedTypeSymbol union,
        IReadOnlyList<INamedTypeSymbol> members
    )
    {
        var builder = new StringBuilder();
        Header(builder);
        OpenNamespace(builder, union);
        Line(builder, 1, Declaration(union) + " : global::Universe.Carotene.Union.IUnion");
        Line(builder, 1, "{");

        foreach (var member in members)
        {
            var typeName =
                member.TypeKind == TypeKind.Struct ? TypeName(member) : TypeName(member) + "?";
            Line(builder, 2, "private readonly " + typeName + " " + FieldName(member) + ";");
        }

        Line(builder, 0, string.Empty);

        foreach (var member in members)
        {
            Constructor(builder, union, members, member);
            Line(builder, 0, string.Empty);
        }

        foreach (var member in members)
        {
            Line(
                builder,
                2,
                "public static "
                    + TypeName(union)
                    + " "
                    + Id(member.Name)
                    + "("
                    + TypeName(member)
                    + " value) => new(Kind."
                    + Id(member.Name)
                    + ", value);"
            );
        }

        Line(builder, 0, string.Empty);
        Line(builder, 2, "public Kind Tag { get; }");
        Line(builder, 0, string.Empty);
        VoidMatch(builder, union, members);
        Line(builder, 0, string.Empty);
        ResultMatch(builder, union, members);
        Line(builder, 0, string.Empty);

        // if (hasStorage)
        // {
        //     GetValue(builder);
        //     Line(builder, 0, string.Empty);
        // }

        Implicit(builder, union, members);
        Line(builder, 0, string.Empty);
        Line(builder, 2, "public enum Kind");
        Line(builder, 2, "{");
        Line(builder, 3, "None,");
        foreach (var member in members)
        {
            Line(builder, 3, Id(member.Name) + ",");
        }

        Line(builder, 2, "}");

        // if (hasStorage)
        // {
        //     Line(builder, 0, string.Empty);
        //     Storage(builder, maxSize);
        // }

        Line(builder, 1, "}");
        CloseNamespace(builder, union);
        return builder.ToString();
    }

    private static void Constructor(
        StringBuilder builder,
        INamedTypeSymbol union,
        IReadOnlyList<INamedTypeSymbol> members,
        INamedTypeSymbol activeMember
    )
    {
        var parameterType = TypeName(activeMember);
        var parameter =
            activeMember.TypeKind == TypeKind.Class
                ? "global::System.String value".Equals(parameterType, StringComparison.Ordinal)
                    ? parameterType + " value"
                    : parameterType + " value"
                : parameterType + " value";

        Line(builder, 2, "private " + Id(union.Name) + "(Kind kind, in " + parameter + ")");
        Line(builder, 2, "{");

        Line(builder, 3, "Tag = kind;");

        foreach (var member in members)
        {
            // For class unions, only the active field needs to be assigned.
            // Unassigned reference fields are initialized to null automatically.
            if (
                union.TypeKind == TypeKind.Class
                && !SymbolEqualityComparer.Default.Equals(member, activeMember)
            )
            {
                continue;
            }

            var value = SymbolEqualityComparer.Default.Equals(member, activeMember)
                ? "value"
                : "default";

            Line(builder, 3, FieldName(member) + " = " + value + ";");
        }

        Line(builder, 2, "}");
    }

    private static void Implicit(
        StringBuilder builder,
        INamedTypeSymbol union,
        IReadOnlyList<INamedTypeSymbol> members
    )
    {
        foreach (var member in members)
        {
            Line(
                builder,
                2,
                "public static implicit operator "
                    + TypeName(union)
                    + "("
                    + TypeName(member)
                    + " value) => "
                    + Id(member.Name)
                    + "(value);"
            );
        }
    }

    private static void VoidMatch(
        StringBuilder builder,
        INamedTypeSymbol union,
        IReadOnlyList<INamedTypeSymbol> members
    )
    {
        var arguments = string.Join(
            ", ",
            members.Select(member => VoidDelegate(member) + " " + ParameterName(member))
        );

        Line(builder, 2, "public void Match(" + arguments + ")");
        Line(builder, 2, "{");
        Line(builder, 3, "switch (Tag)");
        Line(builder, 3, "{");
        Line(
            builder,
            4,
            "case Kind.None: throw new global::System.InvalidOperationException(\"Cannot match an empty "
                + union.Name
                + ".\");"
        );

        foreach (var member in members)
        {
            Line(builder, 4, "case Kind." + Id(member.Name) + ":");
            Line(builder, 5, Invocation(member, ParameterName(member)) + ";");
            Line(builder, 5, "return;");
        }

        Line(
            builder,
            4,
            "default: throw new global::System.InvalidOperationException(\"Unknown "
                + union.Name
                + ".Kind: \" + Tag + \".\");"
        );
        Line(builder, 3, "}");
        Line(builder, 2, "}");
    }

    private static void ResultMatch(
        StringBuilder builder,
        INamedTypeSymbol union,
        IReadOnlyList<INamedTypeSymbol> members
    )
    {
        var arguments = string.Join(
            ", ",
            members.Select(member => ResultDelegate(member) + " " + ParameterName(member))
        );

        Line(builder, 2, "public TResult Match<TResult>(" + arguments + ")");
        Line(builder, 2, "{");
        Line(builder, 3, "return Tag switch");
        Line(builder, 3, "{");
        Line(
            builder,
            4,
            "Kind.None => throw new global::System.InvalidOperationException(\"Cannot match an empty "
                + union.Name
                + ".\"),"
        );

        foreach (var member in members)
        {
            Line(
                builder,
                4,
                "Kind." + Id(member.Name) + " => " + Invocation(member, ParameterName(member)) + ","
            );
        }

        Line(
            builder,
            4,
            "_ => throw new global::System.InvalidOperationException(\"Unknown "
                + union.Name
                + ".Kind: \" + Tag + \".\"),"
        );
        Line(builder, 3, "};");
        Line(builder, 2, "}");
    }

    private static string Invocation(INamedTypeSymbol member, string handler)
    {
        string argument;

        if (member.TypeKind == TypeKind.Struct)
        {
            argument =
                member.TypeKind == TypeKind.Struct
                    ? "in " + FieldName(member)
                    : FieldName(member) + "!";
        }
        else
        {
            argument =
                member.TypeKind == TypeKind.Class ? FieldName(member) + "!" : FieldName(member);
        }

        return handler + "(" + argument + ")";
    }

    private static bool IsValueType(INamedTypeSymbol member) =>
        member.TypeKind == TypeKind.Struct && member.IsValueType;

    private static void GetValue(StringBuilder builder)
    {
        Line(
            builder,
            2,
            "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]"
        );
        Line(
            builder,
            2,
            "private ref readonly T Get<T>() where T : struct => ref global::System.Runtime.CompilerServices.Unsafe.As<Storage, T>(ref global::System.Runtime.CompilerServices.Unsafe.AsRef(in _storage));"
        );
    }

    private static void Storage(StringBuilder builder, int size)
    {
        Line(
            builder,
            2,
            "[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit, Size = "
                + size
                + ")]"
        );
        Line(builder, 2, "private readonly struct Storage");
        Line(builder, 2, "{");
        Line(builder, 3, "public static Storage Create<T>(T value) where T : struct");
        Line(builder, 3, "{");
        Line(builder, 4, "Storage storage = default;");
        Line(
            builder,
            4,
            "global::System.Runtime.CompilerServices.Unsafe.As<Storage, T>(ref storage) = value;"
        );
        Line(builder, 4, "return storage;");
        Line(builder, 3, "}");
        Line(builder, 2, "}");
    }

    // private static string GenerateMember(INamedTypeSymbol union, INamedTypeSymbol member)
    // {
    //     var builder = new StringBuilder();
    //     Header(builder);
    //     OpenNamespace(builder, member);
    //     Line(builder, 0, Declaration(member) + " : " + TypeName(union) + "." + MarkerName(union));
    //     Line(builder, 0, "{");
    //     Line(builder, 0, "}");
    //     CloseNamespace(builder, member);
    //     return builder.ToString();
    // }

    private static string VoidDelegate(INamedTypeSymbol member)
    {
        return member.TypeKind == TypeKind.Struct
            ? "global::Universe.Carotene.Union.MatchFunc<" + TypeName(member) + ">"
            : "global::System.Action<" + TypeName(member) + ">";
    }

    private static string ResultDelegate(INamedTypeSymbol member)
    {
        return member.TypeKind == TypeKind.Struct
            ? "global::Universe.Carotene.Union.MatchFunc<" + TypeName(member) + ", TResult>"
            : "global::System.Func<" + TypeName(member) + ", TResult>";
    }

    private static string Declaration(INamedTypeSymbol symbol)
    {
        if (symbol.TypeKind == TypeKind.Struct)
        {
            return HasModifier(symbol, SyntaxKind.ReadOnlyKeyword)
                ? "readonly partial struct " + Id(symbol.Name)
                : "partial struct " + Id(symbol.Name);
        }

        return (symbol.IsSealed ? "sealed partial " : "partial ")
            + (IsRecord(symbol) ? "record " : "class ")
            + Id(symbol.Name);
    }

    private static bool IsRecord(INamedTypeSymbol symbol)
    {
        return symbol.DeclaringSyntaxReferences.Any(reference =>
            reference.GetSyntax() is RecordDeclarationSyntax
        );
    }

    private static bool HasModifier(INamedTypeSymbol symbol, SyntaxKind kind)
    {
        return symbol.DeclaringSyntaxReferences.Any(reference =>
            reference.GetSyntax() is TypeDeclarationSyntax declaration
            && declaration.Modifiers.Any(modifier => modifier.IsKind(kind))
        );
    }

    private static string TypeName(ITypeSymbol symbol)
    {
        return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    private static string FieldName(INamedTypeSymbol symbol)
    {
        return "_" + char.ToLowerInvariant(symbol.Name[0]) + symbol.Name.Substring(1);
    }

    private static string ParameterName(INamedTypeSymbol symbol)
    {
        return Id(char.ToLowerInvariant(symbol.Name[0]) + symbol.Name.Substring(1));
    }

    private static string Id(string value)
    {
        return "@" + value;
    }

    private static string HintName(INamedTypeSymbol symbol, string suffix)
    {
        var name = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return string.Concat(
                name.Select(character => char.IsLetterOrDigit(character) ? character : '_')
            )
            + "_"
            + suffix
            + ".g.cs";
    }

    private static void Header(StringBuilder builder)
    {
        builder.AppendLine("// <auto-generated />");
        builder.AppendLine("#nullable enable");
        builder.AppendLine();
    }

    private static void OpenNamespace(StringBuilder builder, INamedTypeSymbol symbol)
    {
        if (!symbol.ContainingNamespace.IsGlobalNamespace)
        {
            builder.Append("namespace ");
            builder.AppendLine(symbol.ContainingNamespace.ToDisplayString());
            builder.AppendLine("{");
        }
    }

    private static void CloseNamespace(StringBuilder builder, INamedTypeSymbol symbol)
    {
        if (!symbol.ContainingNamespace.IsGlobalNamespace)
        {
            builder.AppendLine("}");
        }
    }

    private static void Line(StringBuilder builder, int indent, string text)
    {
        if (text.Length > 0)
        {
            builder.Append(' ', indent * 4);
            builder.Append(text);
        }

        builder.AppendLine();
    }

    private static DiagnosticDescriptor Descriptor(string id, string title, string message)
    {
        return new DiagnosticDescriptor(
            id,
            title,
            message,
            "Carotene.Union",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
    }

    private sealed class Candidate(
        INamedTypeSymbol symbol,
        ImmutableArray<AttributeData> attributes
    )
    {
        public INamedTypeSymbol Symbol { get; } = symbol;

        public ImmutableArray<AttributeData> Attributes { get; } = attributes;
    }
}
