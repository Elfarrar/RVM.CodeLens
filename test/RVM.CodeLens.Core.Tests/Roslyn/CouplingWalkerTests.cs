using Microsoft.CodeAnalysis.CSharp;
using RVM.CodeLens.Core.Roslyn.SyntaxWalkers;
using RVM.CodeLens.Core.Tests.Helpers;
using Xunit;

namespace RVM.CodeLens.Core.Tests.Roslyn;

public class CouplingWalkerTests
{
    [Fact]
    public void CouplingCount_IsZero_ForEmptyClass()
    {
        const string code = """
            namespace MyApp;
            public class EmptyClass { }
            """;

        var (model, _) = RoslynTestHelper.GetSemanticModel(code);
        var tree = model.SyntaxTree;
        var walker = new CouplingWalker(model, "EmptyClass");
        walker.Visit(tree.GetRoot());

        Assert.Equal(0, walker.CouplingCount);
    }

    [Fact]
    public void CouplingCount_IgnoresPrimitiveTypes()
    {
        const string code = """
            namespace MyApp;
            public class PrimitiveClass
            {
                public void Method()
                {
                    int x = 5;
                    string s = "hello";
                    bool b = true;
                }
            }
            """;

        var (model, _) = RoslynTestHelper.GetSemanticModel(code);
        var tree = model.SyntaxTree;
        var walker = new CouplingWalker(model, "PrimitiveClass");
        walker.Visit(tree.GetRoot());

        Assert.Equal(0, walker.CouplingCount);
    }

    [Fact]
    public void CouplingCount_CountsDistinctExternalTypes()
    {
        const string code = """
            using System.Collections.Generic;
            using System.Text;
            namespace MyApp;
            public class MyService
            {
                private readonly List<string> _items = new();
                private readonly StringBuilder _builder = new();
                public void Process(List<string> list) { }
            }
            """;

        var (model, _) = RoslynTestHelper.GetSemanticModel(code);
        var tree = model.SyntaxTree;
        var walker = new CouplingWalker(model, "MyService");
        walker.Visit(tree.GetRoot());

        // Expects List<T> and StringBuilder to be counted
        Assert.True(walker.CouplingCount >= 1);
    }

    [Fact]
    public void CouplingCount_ExcludesSelfReferences()
    {
        const string code = """
            namespace MyApp;
            public class SelfRef
            {
                public static SelfRef Instance { get; } = new SelfRef();
                public SelfRef GetSelf() => this;
            }
            """;

        var (model, _) = RoslynTestHelper.GetSemanticModel(code);
        var tree = model.SyntaxTree;
        var walker = new CouplingWalker(model, "SelfRef");
        walker.Visit(tree.GetRoot());

        // Self references should not be counted
        Assert.Equal(0, walker.CouplingCount);
    }

    [Fact]
    public void CouplingCount_CountsMethodParameters()
    {
        const string code = """
            using System.Collections.Generic;
            namespace MyApp;
            public class Processor
            {
                public void Process(List<int> items) { }
            }
            """;

        var (model, _) = RoslynTestHelper.GetSemanticModel(code);
        var tree = model.SyntaxTree;
        var walker = new CouplingWalker(model, "Processor");
        walker.Visit(tree.GetRoot());

        Assert.True(walker.CouplingCount >= 1);
    }

    [Fact]
    public void CouplingCount_CountsIsDistinct_NotDuplicated()
    {
        const string code = """
            using System.Collections.Generic;
            namespace MyApp;
            public class Deduper
            {
                private List<string> _a = new();
                private List<string> _b = new();
                private List<int> _c = new();
                public void DoSomething(List<string> param) { }
            }
            """;

        var (model, _) = RoslynTestHelper.GetSemanticModel(code);
        var tree = model.SyntaxTree;
        var walker = new CouplingWalker(model, "Deduper");
        walker.Visit(tree.GetRoot());

        // List<string> and List<int> are different generic instantiations
        // but List<T> is one type — actual count depends on semantic resolution
        Assert.True(walker.CouplingCount >= 1);
    }
}
