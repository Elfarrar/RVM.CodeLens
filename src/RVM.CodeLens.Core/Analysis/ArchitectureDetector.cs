using RVM.CodeLens.Core.Models;

namespace RVM.CodeLens.Core.Analysis;

public class ArchitectureDetector : IArchitectureDetector
{
    private static readonly Dictionary<string, string[]> LayerKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Domain"] = ["Domain", "Core", "Entities", "Model"],
        ["Application"] = ["Application", "Service", "Services", "UseCases"],
        ["Infrastructure"] = ["Infrastructure", "Data", "Persistence", "Repository"],
        ["Presentation"] = ["API", "Web", "UI", "Blazor", "MVC", "CLI"],
        ["Tests"] = ["Test", "Tests", "Spec", "Specs"],
        ["Shared"] = ["Shared", "Common", "Contracts", "Abstractions"]
    };

    // Allowed reference directions (source can reference target)
    private static readonly Dictionary<string, HashSet<string>> AllowedReferences = new()
    {
        ["Presentation"] = ["Application", "Domain", "Shared", "Infrastructure"],
        ["Application"] = ["Domain", "Shared"],
        ["Infrastructure"] = ["Domain", "Application", "Shared"],
        ["Domain"] = ["Shared"],
        ["Tests"] = ["Presentation", "Application", "Infrastructure", "Domain", "Shared"],
        ["Shared"] = []
    };

    public ArchitectureAnalysis Detect(List<ProjectAnalysis> projects)
    {
        var layers = new Dictionary<string, List<string>>();
        var projectLayerMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var project in projects)
        {
            var layer = DetectLayer(project.Name);
            if (!layers.ContainsKey(layer))
                layers[layer] = [];
            layers[layer].Add(project.Name);
            projectLayerMap[project.Name] = layer;
        }

        var violations = new List<string>();

        foreach (var project in projects)
        {
            if (!projectLayerMap.TryGetValue(project.Name, out var sourceLayer))
                continue;

            foreach (var reference in project.ProjectReferences)
            {
                if (!projectLayerMap.TryGetValue(reference, out var targetLayer))
                    continue;

                if (sourceLayer == targetLayer)
                    continue;

                if (!AllowedReferences.TryGetValue(sourceLayer, out var allowed) ||
                    !allowed.Contains(targetLayer))
                {
                    violations.Add($"{project.Name} ({sourceLayer}) → {reference} ({targetLayer})");
                }
            }
        }

        var architectureLayers = layers
            .Select(kvp => new ArchitectureLayer(kvp.Key, kvp.Value))
            .OrderBy(l => l.Name switch
            {
                "Presentation" => 0,
                "Application" => 1,
                "Domain" => 2,
                "Infrastructure" => 3,
                "Shared" => 4,
                "Tests" => 5,
                _ => 6
            })
            .ToList();

        return new ArchitectureAnalysis(architectureLayers, violations);
    }

    private static string DetectLayer(string projectName)
    {
        foreach (var (layer, keywords) in LayerKeywords)
        {
            foreach (var keyword in keywords)
            {
                if (projectName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    return layer;
            }
        }

        return "Other";
    }
}
