using RVM.CodeLens.Core.Roslyn.SyntaxWalkers;
using RVM.CodeLens.Core.Tests.Helpers;
using Xunit;

namespace RVM.CodeLens.Core.Tests.Roslyn;

public class LineCountWalkerTests
{
    [Fact]
    public void Should_Count_Simple_File()
    {
        var code = """
            class C
            {
                void M() { }
            }
            """;

        var tree = RoslynTestHelper.Parse(code);
        var counts = LineCountWalker.Count(tree);

        Assert.Equal(4, counts.TotalLines);
        Assert.True(counts.CodeLines > 0);
        Assert.Equal(0, counts.CommentLines);
    }

    [Fact]
    public void Should_Count_Comment_Lines()
    {
        var code = """
            // This is a comment
            class C
            {
                // Another comment
                void M() { }
            }
            """;

        var tree = RoslynTestHelper.Parse(code);
        var counts = LineCountWalker.Count(tree);

        Assert.Equal(2, counts.CommentLines);
    }

    [Fact]
    public void Should_Count_Blank_Lines()
    {
        var code = "class C\n{\n\n    void M() { }\n\n}\n";

        var tree = RoslynTestHelper.Parse(code);
        var counts = LineCountWalker.Count(tree);

        Assert.True(counts.BlankLines >= 2, $"Expected at least 2 blank lines, got {counts.BlankLines}");
    }

    [Fact]
    public void Should_Count_Multiline_Comments()
    {
        var code = """
            /*
             * Multi-line
             * comment
             */
            class C { }
            """;

        var tree = RoslynTestHelper.Parse(code);
        var counts = LineCountWalker.Count(tree);

        Assert.True(counts.CommentLines >= 3, $"Expected at least 3 comment lines, got {counts.CommentLines}");
    }
}
