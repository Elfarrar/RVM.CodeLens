using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RVM.CodeLens.Core.Roslyn.SyntaxWalkers;

/// <summary>
/// Counts distinct types referenced by a type declaration using semantic analysis.
/// </summary>
public class CouplingWalker : CSharpSyntaxWalker
{
    private readonly SemanticModel _semanticModel;
    private readonly HashSet<string> _referencedTypes = new();
    private readonly string _currentTypeName;

    public int CouplingCount => _referencedTypes.Count;

    public CouplingWalker(SemanticModel semanticModel, string currentTypeName)
    {
        _semanticModel = semanticModel;
        _currentTypeName = currentTypeName;
    }

    public override void VisitIdentifierName(IdentifierNameSyntax node)
    {
        TryAddType(node);
        base.VisitIdentifierName(node);
    }

    public override void VisitGenericName(GenericNameSyntax node)
    {
        TryAddType(node);
        base.VisitGenericName(node);
    }

    public override void VisitQualifiedName(QualifiedNameSyntax node)
    {
        TryAddType(node);
        base.VisitQualifiedName(node);
    }

    public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        TryAddType(node.Type);
        base.VisitObjectCreationExpression(node);
    }

    private void TryAddType(SyntaxNode node)
    {
        var symbolInfo = _semanticModel.GetSymbolInfo(node);
        var symbol = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();

        INamedTypeSymbol? typeSymbol = symbol switch
        {
            INamedTypeSymbol t => t,
            IMethodSymbol m => m.ContainingType,
            IPropertySymbol p => p.ContainingType,
            IFieldSymbol f => f.ContainingType,
            _ => null
        };

        if (typeSymbol is null) return;
        if (typeSymbol.SpecialType != SpecialType.None) return; // skip primitives
        if (typeSymbol.Name == _currentTypeName) return; // skip self

        var fullName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        _referencedTypes.Add(fullName);
    }
}
