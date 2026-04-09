using RVM.CodeLens.Core.Models;

namespace RVM.CodeLens.Core.Analysis;

public class DependencyGraphBuilder : IDependencyGraphBuilder
{
    public DependencyGraph Build(List<ProjectAnalysis> projects)
    {
        var nodes = new List<DependencyNode>();
        var edges = new List<DependencyEdge>();
        var addedPackages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Add project nodes
        foreach (var project in projects)
        {
            nodes.Add(new DependencyNode(project.Name, project.Name, "project"));
        }

        var projectNames = projects.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var project in projects)
        {
            // Project-to-project references
            foreach (var reference in project.ProjectReferences)
            {
                if (projectNames.Contains(reference))
                {
                    edges.Add(new DependencyEdge(project.Name, reference, "reference"));
                }
            }

            // NuGet package references
            foreach (var package in project.PackageReferences)
            {
                var packageId = $"pkg:{package.Name}";
                if (addedPackages.Add(packageId))
                {
                    nodes.Add(new DependencyNode(packageId, $"{package.Name} ({package.Version})", "package"));
                }

                edges.Add(new DependencyEdge(project.Name, packageId, "package"));
            }
        }

        return new DependencyGraph(nodes, edges);
    }
}
