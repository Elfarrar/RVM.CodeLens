using RVM.CodeLens.Core.Models;

namespace RVM.CodeLens.Core.Analysis;

public interface IDependencyGraphBuilder
{
    DependencyGraph Build(List<ProjectAnalysis> projects);
}
