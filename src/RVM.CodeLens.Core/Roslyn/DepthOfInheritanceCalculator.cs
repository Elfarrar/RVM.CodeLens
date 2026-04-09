using Microsoft.CodeAnalysis;

namespace RVM.CodeLens.Core.Roslyn;

public static class DepthOfInheritanceCalculator
{
    public static int Calculate(INamedTypeSymbol typeSymbol)
    {
        var depth = 0;
        var current = typeSymbol.BaseType;

        while (current is not null && current.SpecialType != SpecialType.System_Object)
        {
            depth++;
            current = current.BaseType;
        }

        return depth;
    }
}
