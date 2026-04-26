# Testes — RVM.CodeLens

## Testes Unitarios
- **Framework:** xUnit + Moq
- **Localizacao:** `test/RVM.CodeLens.Core.Tests/`
- **Total:** 58 testes
- **Foco:** walkers Roslyn, metricas, servicos de analise

```bash
dotnet test test/RVM.CodeLens.Core.Tests/
```

## Testes E2E (Playwright)
- **Localizacao:** `test/playwright/`
- **Cobertura:** dashboard Blazor (upload de projeto, visualizacao de metricas, grafos D3.js)

```bash
cd test/playwright
npm install
npx playwright install --with-deps
npx playwright test
```

Variaveis de ambiente necessarias:
```
CODELENS_BASE_URL=http://localhost:5000
```

## CI
- **Arquivo:** `.github/workflows/ci.yml`
- Pipeline: build → testes unitarios → Playwright
- Nota: CI nao roda analise real de projetos (sem MSBuild disponivel no runner) — testes usam mocks
