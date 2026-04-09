***English** | [Português](README.md)*

# RVM.CodeLens

Static analysis tool for .NET solutions that calculates code metrics, detects architectural patterns, maps dependencies, and identifies hot spots via Git history.

![build](https://img.shields.io/badge/build-passing-brightgreen)
![tests](https://img.shields.io/badge/tests-24%20passed-brightgreen)
![license](https://img.shields.io/badge/license-MIT-blue)
![dotnet](https://img.shields.io/badge/.NET-10.0-purple)

---

## About

**RVM.CodeLens** analyzes .NET solutions (.sln / .slnx) using the Roslyn platform to extract code quality metrics in an automated way. The project offers three usage interfaces:

- **CLI** -- command line with table, JSON, or Markdown output
- **Web** -- Blazor Server dashboard with interactive charts
- **REST API** -- endpoints for integration with other tools

### What it measures

| Metric | Description |
|--------|-------------|
| Cyclomatic Complexity (CC) | Number of independent paths in each method |
| Maintainability Index (MI) | Visual Studio formula: combines Halstead volume, CC, and LOC |
| Class Coupling | Distinct types referenced per method (semantic analysis) |
| Depth of Inheritance | Inheritance levels down to `System.Object` |
| Line Count | Total, code, comments, and blank lines |
| Dependency Graph | References between projects and NuGet packages |
| Architecture Detection | Identifies layers (Domain, Application, Infrastructure, Presentation) and violations |
| Hot Spots | Crosses Git churn (commits x authors) with complexity to find high-risk files |

---

## Technologies

- **.NET 10** -- target framework for all projects
- **Roslyn (Microsoft.CodeAnalysis)** 5.3 -- syntactic and semantic analysis of C#
- **MSBuild Workspaces** -- loading solutions and projects
- **LibGit2Sharp** 0.30 -- reading Git history for hot spots
- **Spectre.Console** 0.49 -- tables and rich terminal UI
- **Blazor Server** -- web dashboard with Interactive Server Components
- **Serilog** 10.0 -- structured logging (CompactJsonFormatter)
- **xUnit** 2.9 + **Moq** 4.20 -- unit tests
- **Coverlet** 6.0 -- code coverage

---

## Architecture

```
┌─────────────────────────────────────────────────────┐
│                    Presentation                     │
│  ┌──────────────────┐    ┌───────────────────────┐  │
│  │  RVM.CodeLens.CLI│    │  RVM.CodeLens.Web     │  │
│  │  (Spectre.Console)│    │  (Blazor Server + API)│  │
│  └────────┬─────────┘    └──────────┬────────────┘  │
│           │                         │               │
│           └────────────┬────────────┘               │
│                        │                            │
├────────────────────────┼────────────────────────────┤
│                        ▼                            │
│              RVM.CodeLens.Core                      │
│  ┌──────────────────────────────────────────────┐   │
│  │  Analysis/                                   │   │
│  │   SolutionAnalyzer -> ProjectAnalyzer        │   │
│  │   MetricsCalculator   GitAnalyzer            │   │
│  │   DependencyGraphBuilder                     │   │
│  │   ArchitectureDetector                       │   │
│  ├──────────────────────────────────────────────┤   │
│  │  Roslyn/                                     │   │
│  │   ComplexityWalker   CouplingWalker          │   │
│  │   LineCountWalker    DepthOfInheritance      │   │
│  │   MaintainabilityIndexCalculator             │   │
│  ├──────────────────────────────────────────────┤   │
│  │  Workspace/                                  │   │
│  │   SolutionLoader   MsBuildInitializer        │   │
│  ├──────────────────────────────────────────────┤   │
│  │  Models/                                     │   │
│  │   SolutionAnalysis  ProjectAnalysis          │   │
│  │   ProjectMetrics    FileMetrics              │   │
│  │   TypeMetrics       MethodMetrics            │   │
│  │   DependencyGraph   ArchitectureAnalysis     │   │
│  │   HotSpot           PackageReference         │   │
│  └──────────────────────────────────────────────┘   │
│                                                     │
├─────────────────────────────────────────────────────┤
│                       Tests                         │
│  ┌──────────────────────────────────────────────┐   │
│  │  RVM.CodeLens.Core.Tests                     │   │
│  │   ArchitectureDetectorTests                  │   │
│  │   DependencyGraphBuilderTests                │   │
│  │   ComplexityWalkerTests                      │   │
│  │   LineCountWalkerTests                       │   │
│  │   MaintainabilityIndexTests                  │   │
│  └──────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

---

## Project Structure

```
RVM.CodeLens/
├── RVM.CodeLens.slnx
├── global.json                          # SDK .NET 10.0.201
├── src/
│   ├── RVM.CodeLens.Core/              # Main library
│   │   ├── Analysis/
│   │   │   ├── SolutionAnalyzer.cs     # Orchestrates full analysis
│   │   │   ├── ProjectAnalyzer.cs      # Per-project analysis
│   │   │   ├── MetricsCalculator.cs    # Metrics via Roslyn
│   │   │   ├── DependencyGraphBuilder.cs
│   │   │   ├── ArchitectureDetector.cs # Layers + violations
│   │   │   ├── GitAnalyzer.cs         # Hot spots via LibGit2Sharp
│   │   │   └── I*.cs                  # Interfaces (6)
│   │   ├── Models/                    # Immutable records
│   │   ├── Roslyn/
│   │   │   ├── SyntaxWalkers/         # ComplexityWalker, CouplingWalker, LineCountWalker
│   │   │   ├── MaintainabilityIndexCalculator.cs
│   │   │   └── DepthOfInheritanceCalculator.cs
│   │   ├── Workspace/                 # SolutionLoader, MsBuildInitializer
│   │   └── DependencyInjection.cs     # AddCodeLensCore()
│   │
│   ├── RVM.CodeLens.CLI/              # Console application
│   │   ├── Program.cs                 # Entry point (Spectre.Console.Cli)
│   │   ├── Commands/                  # analyze, metrics, deps, hotspots, architecture
│   │   └── Formatters/               # TableFormatter, JsonFormatter, MarkdownFormatter
│   │
│   └── RVM.CodeLens.Web/             # Web dashboard
│       ├── Program.cs                # ASP.NET + Blazor Server + Serilog
│       ├── Api/
│       │   └── AnalysisEndpoints.cs  # Minimal API (6 endpoints)
│       ├── Services/
│       │   └── AnalysisStateService.cs
│       └── Components/
│           └── Pages/                # Home, Dashboard, Metrics, HotSpots,
│                                     # DependencyGraph, Architecture
│
└── test/
    └── RVM.CodeLens.Core.Tests/      # 24 tests (xUnit + Moq)
        ├── Analysis/
        ├── Roslyn/
        └── Helpers/
```

---

## How to Run

### Prerequisites

- [.NET SDK 10.0.201+](https://dotnet.microsoft.com/download)
- Git (for hot spots functionality)
- Visual Studio 2022 / VS Code / Rider (optional)

### Clone and Build

```bash
git clone https://github.com/rvm-tech/RVM.CodeLens.git
cd RVM.CodeLens
dotnet restore
dotnet build
```

### CLI

The executable is called `codelens`.

```bash
# Full analysis of a solution
dotnet run --project src/RVM.CodeLens.CLI -- analyze <path/to/solution.sln>

# Detailed metrics by file and method
dotnet run --project src/RVM.CodeLens.CLI -- metrics <path/to/solution.sln>

# Dependency graph (projects + NuGet)
dotnet run --project src/RVM.CodeLens.CLI -- deps <path/to/solution.sln>

# Hot spots -- files with high change frequency and high complexity
dotnet run --project src/RVM.CodeLens.CLI -- hotspots <path/to/solution.sln> --commits 200

# Architectural layer detection and violations
dotnet run --project src/RVM.CodeLens.CLI -- architecture <path/to/solution.sln>
```

#### Output Formats

```bash
# Formatted table (default)
codelens analyze MySolution.sln

# JSON (for pipelines and integration)
codelens analyze MySolution.sln --format json

# Markdown (for documentation)
codelens analyze MySolution.sln --format markdown
```

### Web (Dashboard)

```bash
dotnet run --project src/RVM.CodeLens.Web
```

Open `http://localhost:5000` in your browser. Enter the solution path on the home screen and navigate through the dashboard.

---

## API Endpoints

All endpoints are under the `/api` prefix.

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/analyze` | Analyzes a solution. Body: `{ "solutionPath": "..." }` |
| `GET` | `/api/analysis/current` | Returns the analysis currently loaded in memory |
| `GET` | `/api/metrics` | Metrics for all projects |
| `GET` | `/api/deps` | Dependency graph (nodes + edges) |
| `GET` | `/api/hotspots` | Hot spots based on Git history |
| `GET` | `/api/architecture` | Detected layers and violations |
| `GET` | `/health` | Health check |

#### Example

```bash
# Analyze solution
curl -X POST http://localhost:5000/api/analyze \
  -H "Content-Type: application/json" \
  -d '{"solutionPath": "C:/projetos/MinhaApp.sln"}'

# Query metrics
curl http://localhost:5000/api/metrics
```

---

## Tests

The project has **24 unit tests** distributed across 5 test classes, covering the critical Core components.

```bash
# Run all tests
dotnet test

# With code coverage (Coverlet)
dotnet test --collect:"XPlat Code Coverage"

# Filter by class
dotnet test --filter "FullyQualifiedName~ComplexityWalkerTests"
```

### Test Coverage

| Test Class | What it tests | Count |
|------------|---------------|:-----:|
| `ComplexityWalkerTests` | Cyclomatic complexity (if, for, while, switch, ??, &&) | 14 |
| `MaintainabilityIndexTests` | Maintainability index (full and simplified formula) | 7 |
| `ArchitectureDetectorTests` | Layer detection and architectural violations | 4 |
| `DependencyGraphBuilderTests` | Dependency graph construction | 4 |
| `LineCountWalkerTests` | Line counting (code, comments, blank) | 4 |

---

## Features

- [x] Loading `.sln` and `.slnx` solutions via MSBuild Workspaces
- [x] Metrics analysis by file, type, and method using Roslyn
- [x] Cyclomatic complexity with support for `if`, `for`, `foreach`, `while`, `do`, `switch`, `catch`, `&&`, `||`, `??`, `?.`, ternary
- [x] Maintainability index (Visual Studio formula with Halstead approximation)
- [x] Class coupling via semantic analysis (symbols)
- [x] Depth of inheritance tree
- [x] Line counting: total, code, comments (single/multi-line), and blank
- [x] Dependency graph between projects and NuGet packages
- [x] Automatic detection of architectural layers (Domain, Application, Infrastructure, Presentation, Tests, Shared)
- [x] Architectural violation validation (e.g., Domain referencing Infrastructure)
- [x] Hot spot analysis via Git history (churn x complexity)
- [x] CLI with 5 commands and 3 output formats (table, JSON, markdown)
- [x] Web dashboard with Blazor Server and interactive charts
- [x] REST API with 6 endpoints (Minimal API)
- [x] CORS configured for integration with Electron app
- [x] Health check endpoint
- [x] Structured logging with Serilog (CompactJsonFormatter)
- [x] Centralized dependency injection (`AddCodeLensCore()`)
- [x] Automatic filtering of generated files (`.g.cs`, `.designer.cs`, `obj/`)
- [x] Unit tests with xUnit, Moq, and Coverlet

---

## CLI Commands

| Command | Description |
|---------|-------------|
| `analyze <PATH>` | Full solution analysis (metrics + architecture) |
| `metrics <PATH>` | Detailed metrics by file and method |
| `deps <PATH>` | Dependency graph (projects and packages) |
| `hotspots <PATH>` | Hot spots: high change frequency + high complexity |
| `architecture <PATH>` | Detects architectural layers and violations |

### Global Options

| Option | Description | Default |
|--------|-------------|---------|
| `-f`, `--format` | Output format: `table`, `json`, `markdown` | `table` |
| `-c`, `--commits` | Number of commits to analyze (`hotspots` only) | `100` |

---

<p align="center">
  Developed by <strong>RVM Tech</strong>
</p>
