using RVM.CodeLens.Core.Roslyn;
using Xunit;

namespace RVM.CodeLens.Core.Tests.Roslyn;

public class MaintainabilityIndexTests
{
    [Fact]
    public void Empty_Method_Should_Return_100()
    {
        var mi = MaintainabilityIndexCalculator.CalculateSimplified(0, 1);
        Assert.Equal(100, mi);
    }

    [Fact]
    public void Simple_Method_Should_Have_High_Maintainability()
    {
        // 5 lines, CC=1
        var mi = MaintainabilityIndexCalculator.CalculateSimplified(5, 1);
        Assert.True(mi > 70, $"Expected MI > 70, got {mi}");
    }

    [Fact]
    public void Complex_Method_Should_Have_Lower_Maintainability()
    {
        // 100 lines, CC=20
        var mi = MaintainabilityIndexCalculator.CalculateSimplified(100, 20);
        Assert.True(mi < 50, $"Expected MI < 50, got {mi}");
    }

    [Fact]
    public void Maintainability_Should_Never_Be_Negative()
    {
        // Extremely complex method
        var mi = MaintainabilityIndexCalculator.CalculateSimplified(1000, 100);
        Assert.True(mi >= 0, $"MI should never be negative, got {mi}");
    }

    [Fact]
    public void Higher_Complexity_Should_Reduce_Maintainability()
    {
        var mi1 = MaintainabilityIndexCalculator.CalculateSimplified(20, 1);
        var mi2 = MaintainabilityIndexCalculator.CalculateSimplified(20, 10);
        Assert.True(mi1 > mi2, $"MI with CC=1 ({mi1}) should be higher than CC=10 ({mi2})");
    }

    [Fact]
    public void More_Lines_Should_Reduce_Maintainability()
    {
        var mi1 = MaintainabilityIndexCalculator.CalculateSimplified(5, 1);
        var mi2 = MaintainabilityIndexCalculator.CalculateSimplified(200, 1);
        Assert.True(mi1 > mi2, $"MI with 5 lines ({mi1}) should be higher than 200 lines ({mi2})");
    }

    [Fact]
    public void Full_Formula_Should_Work()
    {
        var mi = MaintainabilityIndexCalculator.Calculate(10, 2, 15, 8);
        Assert.True(mi > 0 && mi <= 100, $"MI should be between 0-100, got {mi}");
    }
}
