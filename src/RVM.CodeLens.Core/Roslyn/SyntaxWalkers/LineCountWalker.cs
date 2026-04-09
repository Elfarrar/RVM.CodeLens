using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RVM.CodeLens.Core.Roslyn.SyntaxWalkers;

/// <summary>
/// Counts total lines, code lines, blank lines, and comment lines in a syntax tree.
/// </summary>
public static class LineCountWalker
{
    public record LineCounts(int TotalLines, int CodeLines, int BlankLines, int CommentLines);

    public static LineCounts Count(SyntaxTree tree)
    {
        var text = tree.GetText();
        var totalLines = text.Lines.Count;
        var commentLines = new HashSet<int>();
        var blankLines = 0;

        // Find all lines covered by comment trivia
        var root = tree.GetRoot();
        foreach (var trivia in root.DescendantTrivia())
        {
            if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
            {
                var span = trivia.Span;
                var startLine = text.Lines.GetLineFromPosition(span.Start).LineNumber;
                var endLine = text.Lines.GetLineFromPosition(span.End).LineNumber;
                for (var i = startLine; i <= endLine; i++)
                    commentLines.Add(i);
            }
        }

        // Count blank lines (lines with only whitespace that aren't comment lines)
        for (var i = 0; i < text.Lines.Count; i++)
        {
            var line = text.Lines[i];
            if (string.IsNullOrWhiteSpace(line.ToString()) && !commentLines.Contains(i))
                blankLines++;
        }

        var codeLines = totalLines - blankLines - commentLines.Count;

        return new LineCounts(totalLines, Math.Max(0, codeLines), blankLines, commentLines.Count);
    }
}
