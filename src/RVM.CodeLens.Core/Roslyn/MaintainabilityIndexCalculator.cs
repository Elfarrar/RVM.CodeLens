namespace RVM.CodeLens.Core.Roslyn;

/// <summary>
/// Calculates the Maintainability Index using the Visual Studio formula:
/// MI = max(0, (171 - 5.2 * ln(HV) - 0.23 * CC - 16.2 * ln(LOC)) * 100 / 171)
///
/// Where HV = Halstead Volume (approximated), CC = Cyclomatic Complexity, LOC = Lines of Code.
/// </summary>
public static class MaintainabilityIndexCalculator
{
    public static double Calculate(int linesOfCode, int cyclomaticComplexity, int uniqueOperands, int uniqueOperators)
    {
        if (linesOfCode <= 0) return 100;

        var halsteadVolume = CalculateHalsteadVolume(uniqueOperands, uniqueOperators);
        var lnHv = halsteadVolume > 0 ? Math.Log(halsteadVolume) : 0;
        var lnLoc = Math.Log(linesOfCode);

        var mi = (171.0 - 5.2 * lnHv - 0.23 * cyclomaticComplexity - 16.2 * lnLoc) * 100.0 / 171.0;

        return Math.Max(0, Math.Round(mi, 2));
    }

    /// <summary>
    /// Simplified calculation using only LOC and CC (when Halstead metrics are unavailable).
    /// </summary>
    public static double CalculateSimplified(int linesOfCode, int cyclomaticComplexity)
    {
        if (linesOfCode <= 0) return 100;

        // Approximate Halstead Volume as LOC * log2(LOC)
        var approxHv = linesOfCode * Math.Log2(Math.Max(2, linesOfCode));
        var lnHv = Math.Log(approxHv);
        var lnLoc = Math.Log(linesOfCode);

        var mi = (171.0 - 5.2 * lnHv - 0.23 * cyclomaticComplexity - 16.2 * lnLoc) * 100.0 / 171.0;

        return Math.Max(0, Math.Round(mi, 2));
    }

    private static double CalculateHalsteadVolume(int uniqueOperands, int uniqueOperators)
    {
        var vocabulary = uniqueOperands + uniqueOperators;
        if (vocabulary <= 0) return 0;

        var programLength = uniqueOperands + uniqueOperators; // simplified
        return programLength * Math.Log2(vocabulary);
    }
}
