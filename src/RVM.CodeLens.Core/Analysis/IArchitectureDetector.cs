using RVM.CodeLens.Core.Models;

namespace RVM.CodeLens.Core.Analysis;

public interface IArchitectureDetector
{
    ArchitectureAnalysis Detect(List<ProjectAnalysis> projects);
}
