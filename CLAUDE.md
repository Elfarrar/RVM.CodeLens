# RVM.CodeLens

## Visao Geral
Ferramenta de analise estatica de codigo .NET usando Roslyn e MSBuild Workspace. Extrai metricas (complexidade, acoplamento, cobertura de documentacao), detecta code smells e visualiza dependencias em grafos D3.js interativos.

Projeto portfolio com tres frontends complementares: CLI para uso em terminal/CI, Blazor Web para dashboard interativo, e app Electron (codelens-desktop) para uso offline.

## Stack
- .NET 10, ASP.NET Core, Blazor Server
- Roslyn (Microsoft.CodeAnalysis), MSBuildLocator (workspace MSBuild)
- D3.js (visualizacao de grafos no browser)
- CLI via `System.CommandLine`
- Electron + TypeScript (codelens-desktop)
- Serilog + Seq, RVM.Common.Security
- xUnit 58 testes, Playwright E2E

## Estrutura do Projeto
```
src/
  RVM.CodeLens.Core/          # Motor de analise
    Analysis/                 # Walkers Roslyn (syntax, semantic)
    Models/                   # Metricas, resultados
    Roslyn/                   # Helpers e extensoes Roslyn
    Services/                 # Servicos de analise
    Workspace/                # MsBuildInitializer, workspace loading
    DependencyInjection.cs    # AddCodeLensCore()
  RVM.CodeLens.CLI/           # CLI (dotnet run -- analyze <path>)
    Commands/                 # Subcomandos (analyze, report, export)
    Formatters/               # Saida texto/JSON
  RVM.CodeLens.Web/           # Blazor Server + API endpoints
    Api/                      # MapAnalysisEndpoints() (minimal API)
    Components/               # Blazor pages + D3.js grafos
    Services/                 # AnalysisStateService (estado entre requests)
  codelens-desktop/           # Electron wrapper (TypeScript)
test/
  RVM.CodeLens.Core.Tests/    # xUnit (58 testes)
  playwright/                 # Testes E2E
```

## Convencoes
- `MsBuildInitializer.EnsureInitialized()` DEVE ser chamado antes de qualquer tipo Roslyn
- CORS `ElectronApp` liberado para `http://localhost:5173`
- `AnalysisStateService` e singleton — analises longas ficam em memoria entre requests
- Sem banco de dados: tudo em memoria ou arquivos temporarios
- Security headers aplicados via middleware inline (sem RVM.Common.Security middleware)

## Como Rodar
### Dev
```bash
# Web + Blazor
cd src/RVM.CodeLens.Web
dotnet run

# CLI
cd src/RVM.CodeLens.CLI
dotnet run -- analyze <caminho-do-projeto>

# Desktop (Electron)
cd src/codelens-desktop
npm install && npm run dev
```

### Testes
```bash
dotnet test test/RVM.CodeLens.Core.Tests/
```

## Decisoes Arquiteturais
- **MSBuildLocator antes de carregar tipos Roslyn**: necessidade da API — ordem importa no bootstrap
- **AnalysisStateService como singleton**: analises podem levar segundos; necessario manter estado entre requests SignalR
- **Sem banco de dados**: projeto portfolio, analises sao transientes e nao precisam persistencia
- **Tres frontends**: CLI para DevOps/CI, Blazor para visualizacao rica, Electron para uso offline em rede privada
