*[English](README.en.md) | **Português***

# RVM.CodeLens

Ferramenta de analise estatica para solucoes .NET que calcula metricas de codigo, detecta padroes arquiteturais, mapeia dependencias e identifica hot spots via historico Git.

![build](https://img.shields.io/badge/build-passing-brightgreen)
![tests](https://img.shields.io/badge/tests-24%20passed-brightgreen)
![license](https://img.shields.io/badge/license-MIT-blue)
![dotnet](https://img.shields.io/badge/.NET-10.0-purple)

---

## Sobre

**RVM.CodeLens** analisa solucoes .NET (.sln / .slnx) usando a plataforma Roslyn para extrair metricas de qualidade de codigo de forma automatizada. O projeto oferece tres interfaces de uso:

- **CLI** -- linha de comando com saida em tabela, JSON ou Markdown
- **Web** -- dashboard Blazor Server com graficos interativos
- **API REST** -- endpoints para integracao com outras ferramentas

### O que ele mede

| Metrica | Descricao |
|---------|-----------|
| Complexidade Ciclomatica (CC) | Quantidade de caminhos independentes em cada metodo |
| Indice de Manutenibilidade (MI) | Formula Visual Studio: combina volume Halstead, CC e LOC |
| Acoplamento de Classes | Tipos distintos referenciados por metodo (analise semantica) |
| Profundidade de Heranca | Niveis de heranca ate `System.Object` |
| Contagem de Linhas | Total, codigo, comentarios e linhas em branco |
| Grafo de Dependencias | Referencias entre projetos e pacotes NuGet |
| Deteccao de Arquitetura | Identifica camadas (Domain, Application, Infrastructure, Presentation) e violacoes |
| Hot Spots | Cruza churn Git (commits x autores) com complexidade para encontrar arquivos de risco |

---

## Tecnologias

- **.NET 10** -- target framework de todos os projetos
- **Roslyn (Microsoft.CodeAnalysis)** 5.3 -- analise sintatica e semantica de C#
- **MSBuild Workspaces** -- carregamento de solucoes e projetos
- **LibGit2Sharp** 0.30 -- leitura do historico Git para hot spots
- **Spectre.Console** 0.49 -- tabelas e UI rica no terminal
- **Blazor Server** -- dashboard web com Interactive Server Components
- **Serilog** 10.0 -- logging estruturado (CompactJsonFormatter)
- **xUnit** 2.9 + **Moq** 4.20 -- testes unitarios
- **Coverlet** 6.0 -- cobertura de codigo

---

## Arquitetura

```
┌─────────────────────────────────────────────────────┐
│                    Apresentacao                      │
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
│                      Testes                         │
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

## Estrutura do Projeto

```
RVM.CodeLens/
├── RVM.CodeLens.slnx
├── global.json                          # SDK .NET 10.0.201
├── src/
│   ├── RVM.CodeLens.Core/              # Biblioteca principal
│   │   ├── Analysis/
│   │   │   ├── SolutionAnalyzer.cs     # Orquestra analise completa
│   │   │   ├── ProjectAnalyzer.cs      # Analise por projeto
│   │   │   ├── MetricsCalculator.cs    # Metricas via Roslyn
│   │   │   ├── DependencyGraphBuilder.cs
│   │   │   ├── ArchitectureDetector.cs # Camadas + violacoes
│   │   │   ├── GitAnalyzer.cs         # Hot spots via LibGit2Sharp
│   │   │   └── I*.cs                  # Interfaces (6)
│   │   ├── Models/                    # Records imutaveis
│   │   ├── Roslyn/
│   │   │   ├── SyntaxWalkers/         # ComplexityWalker, CouplingWalker, LineCountWalker
│   │   │   ├── MaintainabilityIndexCalculator.cs
│   │   │   └── DepthOfInheritanceCalculator.cs
│   │   ├── Workspace/                 # SolutionLoader, MsBuildInitializer
│   │   └── DependencyInjection.cs     # AddCodeLensCore()
│   │
│   ├── RVM.CodeLens.CLI/              # Aplicacao console
│   │   ├── Program.cs                 # Ponto de entrada (Spectre.Console.Cli)
│   │   ├── Commands/                  # analyze, metrics, deps, hotspots, architecture
│   │   └── Formatters/               # TableFormatter, JsonFormatter, MarkdownFormatter
│   │
│   └── RVM.CodeLens.Web/             # Dashboard web
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
    └── RVM.CodeLens.Core.Tests/      # 24 testes (xUnit + Moq)
        ├── Analysis/
        ├── Roslyn/
        └── Helpers/
```

---

## Como Executar

### Pre-requisitos

- [.NET SDK 10.0.201+](https://dotnet.microsoft.com/download)
- Git (para funcionalidade de hot spots)
- Visual Studio 2022 / VS Code / Rider (opcional)

### Clone e Build

```bash
git clone https://github.com/rvm-tech/RVM.CodeLens.git
cd RVM.CodeLens
dotnet restore
dotnet build
```

### CLI

O executavel se chama `codelens`.

```bash
# Analise completa de uma solucao
dotnet run --project src/RVM.CodeLens.CLI -- analyze <caminho/para/solucao.sln>

# Metricas detalhadas por arquivo e metodo
dotnet run --project src/RVM.CodeLens.CLI -- metrics <caminho/para/solucao.sln>

# Grafo de dependencias (projetos + NuGet)
dotnet run --project src/RVM.CodeLens.CLI -- deps <caminho/para/solucao.sln>

# Hot spots -- arquivos com alta frequencia de mudanca e alta complexidade
dotnet run --project src/RVM.CodeLens.CLI -- hotspots <caminho/para/solucao.sln> --commits 200

# Deteccao de camadas arquiteturais e violacoes
dotnet run --project src/RVM.CodeLens.CLI -- architecture <caminho/para/solucao.sln>
```

#### Formatos de saida

```bash
# Tabela formatada (padrao)
codelens analyze MySolution.sln

# JSON (para pipelines e integracao)
codelens analyze MySolution.sln --format json

# Markdown (para documentacao)
codelens analyze MySolution.sln --format markdown
```

### Web (Dashboard)

```bash
dotnet run --project src/RVM.CodeLens.Web
```

Acesse `http://localhost:5000` no navegador. Informe o caminho da solucao na tela inicial e navegue pelo dashboard.

---

## Endpoints da API

Todos os endpoints ficam sob o prefixo `/api`.

| Metodo | Rota | Descricao |
|--------|------|-----------|
| `POST` | `/api/analyze` | Analisa uma solucao. Body: `{ "solutionPath": "..." }` |
| `GET` | `/api/analysis/current` | Retorna a analise carregada em memoria |
| `GET` | `/api/metrics` | Metricas de todos os projetos |
| `GET` | `/api/deps` | Grafo de dependencias (nodes + edges) |
| `GET` | `/api/hotspots` | Hot spots com base no historico Git |
| `GET` | `/api/architecture` | Camadas detectadas e violacoes |
| `GET` | `/health` | Health check |

#### Exemplo

```bash
# Analisar solucao
curl -X POST http://localhost:5000/api/analyze \
  -H "Content-Type: application/json" \
  -d '{"solutionPath": "C:/projetos/MinhaApp.sln"}'

# Consultar metricas
curl http://localhost:5000/api/metrics
```

---

## Testes

O projeto possui **24 testes unitarios** distribuidos em 5 classes de teste, cobrindo os componentes criticos do Core.

```bash
# Executar todos os testes
dotnet test

# Com cobertura de codigo (Coverlet)
dotnet test --collect:"XPlat Code Coverage"

# Filtrar por classe
dotnet test --filter "FullyQualifiedName~ComplexityWalkerTests"
```

### Cobertura dos testes

| Classe de Teste | O que testa | Quantidade |
|-----------------|-------------|:----------:|
| `ComplexityWalkerTests` | Complexidade ciclomatica (if, for, while, switch, ??, &&) | 14 |
| `MaintainabilityIndexTests` | Indice de manutenibilidade (formula completa e simplificada) | 7 |
| `ArchitectureDetectorTests` | Deteccao de camadas e violacoes arquiteturais | 4 |
| `DependencyGraphBuilderTests` | Construcao do grafo de dependencias | 4 |
| `LineCountWalkerTests` | Contagem de linhas (codigo, comentarios, branco) | 4 |

---

## Funcionalidades

- [x] Carregamento de solucoes `.sln` e `.slnx` via MSBuild Workspaces
- [x] Analise de metricas por arquivo, tipo e metodo usando Roslyn
- [x] Complexidade ciclomatica com suporte a `if`, `for`, `foreach`, `while`, `do`, `switch`, `catch`, `&&`, `||`, `??`, `?.`, ternario
- [x] Indice de manutenibilidade (formula Visual Studio com aproximacao Halstead)
- [x] Acoplamento de classes via analise semantica (symbols)
- [x] Profundidade de heranca (depth of inheritance tree)
- [x] Contagem de linhas: total, codigo, comentarios (single/multi-line) e em branco
- [x] Grafo de dependencias entre projetos e pacotes NuGet
- [x] Deteccao automatica de camadas arquiteturais (Domain, Application, Infrastructure, Presentation, Tests, Shared)
- [x] Validacao de violacoes arquiteturais (ex: Domain referenciando Infrastructure)
- [x] Analise de hot spots via historico Git (churn x complexidade)
- [x] CLI com 5 comandos e 3 formatos de saida (table, JSON, markdown)
- [x] Dashboard web com Blazor Server e graficos interativos
- [x] API REST com 6 endpoints (Minimal API)
- [x] CORS configurado para integracao com app Electron
- [x] Health check endpoint
- [x] Logging estruturado com Serilog (CompactJsonFormatter)
- [x] Injecao de dependencia centralizada (`AddCodeLensCore()`)
- [x] Filtro automatico de arquivos gerados (`.g.cs`, `.designer.cs`, `obj/`)
- [x] Testes unitarios com xUnit, Moq e Coverlet

---

## Comandos CLI

| Comando | Descricao |
|---------|-----------|
| `analyze <PATH>` | Analise completa da solucao (metricas + arquitetura) |
| `metrics <PATH>` | Metricas detalhadas por arquivo e metodo |
| `deps <PATH>` | Grafo de dependencias (projetos e pacotes) |
| `hotspots <PATH>` | Hot spots: alta frequencia de mudanca + alta complexidade |
| `architecture <PATH>` | Detecta camadas e violacoes arquiteturais |

### Opcoes globais

| Opcao | Descricao | Padrao |
|-------|-----------|--------|
| `-f`, `--format` | Formato de saida: `table`, `json`, `markdown` | `table` |
| `-c`, `--commits` | Quantidade de commits a analisar (apenas `hotspots`) | `100` |

---

<p align="center">
  Desenvolvido por <strong>RVM Tech</strong>
</p>
