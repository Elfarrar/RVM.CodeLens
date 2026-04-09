using RVM.CodeLens.Core.Roslyn.SyntaxWalkers;
using RVM.CodeLens.Core.Tests.Helpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace RVM.CodeLens.Core.Tests.Roslyn;

public class ComplexityWalkerTests
{
    [Theory]
    [InlineData("void M() { }", 1)]
    [InlineData("void M() { if (true) { } }", 2)]
    [InlineData("void M() { if (true) { } else if (false) { } }", 3)]
    [InlineData("void M() { for (int i = 0; i < 10; i++) { } }", 2)]
    [InlineData("void M() { while (true) { } }", 2)]
    [InlineData("void M() { do { } while (true); }", 2)]
    [InlineData("void M() { foreach (var x in new int[0]) { } }", 2)]
    [InlineData("void M() { try { } catch { } }", 2)]
    [InlineData("void M() { var x = true && false; }", 2)]
    [InlineData("void M() { var x = true || false; }", 2)]
    [InlineData("void M() { var x = true ? 1 : 0; }", 2)]
    public void Should_Calculate_Correct_Complexity(string method, int expected)
    {
        var code = $"class C {{ {method} }}";
        var tree = RoslynTestHelper.Parse(code);
        var methodNode = tree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>().First();

        var walker = new ComplexityWalker();
        walker.Visit(methodNode);

        Assert.Equal(expected, walker.Complexity);
    }

    [Fact]
    public void Complex_Method_Should_Have_High_Complexity()
    {
        var code = """
            class C {
                void M(int x) {
                    if (x > 0) {
                        for (int i = 0; i < x; i++) {
                            if (i % 2 == 0 && i > 5) {
                                while (i > 0) {
                                    try { }
                                    catch { }
                                }
                            }
                        }
                    } else if (x < 0) {
                        foreach (var item in new int[0]) { }
                    }
                }
            }
            """;

        var tree = RoslynTestHelper.Parse(code);
        var methodNode = tree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>().First();

        var walker = new ComplexityWalker();
        walker.Visit(methodNode);

        // if + for + if + && + while + catch + else if + foreach = 8 + 1 base = 9
        Assert.Equal(9, walker.Complexity);
    }

    [Fact]
    public void Switch_Expression_Arms_Should_Add_Complexity()
    {
        var code = """
            class C {
                string M(int x) => x switch {
                    1 => "one",
                    2 => "two",
                    _ => "other"
                };
            }
            """;

        var tree = RoslynTestHelper.Parse(code);
        var methodNode = tree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>().First();

        var walker = new ComplexityWalker();
        walker.Visit(methodNode);

        // 1 base + 3 switch arms = 4
        Assert.Equal(4, walker.Complexity);
    }

    [Fact]
    public void Null_Coalescing_Should_Add_Complexity()
    {
        var code = """
            class C {
                string M(string? x) {
                    var result = x ?? "default";
                    return result;
                }
            }
            """;

        var tree = RoslynTestHelper.Parse(code);
        var methodNode = tree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>().First();

        var walker = new ComplexityWalker();
        walker.Visit(methodNode);

        // 1 base + 1 ?? = 2
        Assert.Equal(2, walker.Complexity);
    }
}
