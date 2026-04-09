using Microsoft.CodeAnalysis;

namespace RVM.CodeLens.Core.Workspace;

public interface ISolutionLoader : IDisposable
{
    Task<Solution> LoadAsync(string solutionPath, CancellationToken ct = default);
}
