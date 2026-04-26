/**
 * RVM.CodeLens — Gerador de Manual HTML
 *
 * Le os screenshots gerados pelo Playwright e produz um manual HTML standalone.
 *
 * Uso:
 *   cd docs && npx tsx generate-html.ts
 *
 * Saida:
 *   docs/manual-usuario.html
 *   docs/manual-usuario.md
 */
import fs from 'fs';
import path from 'path';

const SCREENSHOTS_DIR = path.resolve(__dirname, 'screenshots');
const OUTPUT_HTML = path.resolve(__dirname, 'manual-usuario.html');
const OUTPUT_MD = path.resolve(__dirname, 'manual-usuario.md');

interface Section {
  id: string;
  title: string;
  description: string;
  screenshot: string;
  features: string[];
  tips?: string[];
}

const sections: Section[] = [
  {
    id: 'home',
    title: '1. Pagina Inicial',
    description:
      'Tela de entrada do RVM.CodeLens. Apresenta o sistema de analise estatica de codigo ' +
      'e permite iniciar uma nova analise informando o repositorio Git a ser inspecionado.',
    screenshot: '01-home',
    features: [
      'Formulario de entrada de repositorio Git (URL ou caminho local)',
      'Selecao de branch para analise',
      'Historico de analises recentes',
      'Indicadores de status do sistema',
    ],
    tips: [
      'Cole a URL do repositorio GitHub/GitLab diretamente no campo de entrada.',
      'Repositorios privados requerem token de acesso configurado nas definicoes.',
    ],
  },
  {
    id: 'dashboard',
    title: '2. Dashboard de Analise',
    description:
      'Painel central com resumo completo da analise do repositorio. ' +
      'Exibe score de qualidade, contagem de issues por severidade e evolucao historica.',
    screenshot: '02-dashboard',
    features: [
      'Score de qualidade geral (0-100)',
      'Contagem de issues por categoria: bugs, complexidade, duplicacao, seguranca',
      'Grafico de evolucao de qualidade ao longo do tempo',
      'Top 5 arquivos com maior debito tecnico',
      'Resumo de cobertura de testes',
    ],
  },
  {
    id: 'metrics',
    title: '3. Metricas de Codigo',
    description:
      'Analise detalhada das metricas de qualidade do codigo: complexidade ciclomatica, ' +
      'linhas por metodo, acoplamento entre classes e indice de manutenibilidade.',
    screenshot: '03-metrics',
    features: [
      'Complexidade ciclomatica por arquivo e metodo',
      'Linhas de codigo (LOC) e linhas comentadas',
      'Indice de manutenibilidade (0-100)',
      'Acoplamento aferente e eferente por classe',
      'Score CRAP (Change Risk Anti-Patterns)',
      'Filtro e ordenacao por qualquer metrica',
    ],
    tips: [
      'Metodos com complexidade ciclomatica acima de 10 sao candidatos a refatoracao.',
      'Score CRAP acima de 30 indica alto risco de bugs ao alterar o codigo.',
    ],
  },
  {
    id: 'dependencies',
    title: '4. Grafico de Dependencias',
    description:
      'Visualizacao interativa das dependencias entre modulos, namespaces e assemblies. ' +
      'Identifica ciclos de dependencia e acoplamento excessivo.',
    screenshot: '04-dependencies',
    features: [
      'Grafico de dependencias interativo (zoom, pan, filtro)',
      'Deteccao automatica de ciclos de dependencia',
      'Agrupamento por namespace ou assembly',
      'Exportacao do grafico em SVG/PNG',
      'Tabela de dependencias com peso por acoplamento',
    ],
    tips: [
      'Ciclos de dependencia (indicados em vermelho) devem ser eliminados para melhorar a manutenibilidade.',
    ],
  },
  {
    id: 'hotspots',
    title: '5. Hotspots de Risco',
    description:
      'Identificacao dos arquivos e metodos de maior risco: combinacao de alta complexidade ' +
      'com alta frequencia de alteracoes no historico Git.',
    screenshot: '05-hotspots',
    features: [
      'Mapa de calor (heatmap) de arquivos por risco',
      'Combinacao de metricas: complexidade x frequencia de commits',
      'Lista ordenada dos 20 maiores hotspots',
      'Historico de commits por arquivo',
      'Sugestoes de refatoracao priorizadas',
    ],
    tips: [
      'Comece a refatoracao pelos hotspots — eles concentram a maior parte dos bugs em producao.',
    ],
  },
  {
    id: 'architecture',
    title: '6. Visao de Arquitetura',
    description:
      'Analise estrutural do projeto: camadas detectadas automaticamente, ' +
      'violacoes de arquitetura e conformidade com padroes estabelecidos.',
    screenshot: '06-architecture',
    features: [
      'Deteccao automatica de camadas (Domain, Application, Infrastructure, Presentation)',
      'Visualizacao de violacoes de dependencia entre camadas',
      'Conformidade com Clean Architecture / DDD',
      'Analise de responsabilidades por namespace',
      'Relatorio de violacoes exportavel',
    ],
  },
];

// ---------------------------------------------------------------------------
// Utilitarios
// ---------------------------------------------------------------------------
function imageToBase64(filePath: string): string | null {
  if (!fs.existsSync(filePath)) return null;
  const buffer = fs.readFileSync(filePath);
  return `data:image/png;base64,${buffer.toString('base64')}`;
}

function generateHTML(): string {
  const now = new Date().toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });

  let sectionsHtml = '';
  for (const s of sections) {
    const desktopPath = path.join(SCREENSHOTS_DIR, `${s.screenshot}--desktop.png`);
    const mobilePath = path.join(SCREENSHOTS_DIR, `${s.screenshot}--mobile.png`);
    const desktopImg = imageToBase64(desktopPath);
    const mobileImg = imageToBase64(mobilePath);

    const featuresHtml = s.features.map((f) => `<li>${f}</li>`).join('\n            ');
    const tipsHtml = s.tips
      ? `<div class="tips">
          <strong>Dicas:</strong>
          <ul>${s.tips.map((t) => `<li>${t}</li>`).join('\n            ')}</ul>
        </div>`
      : '';

    const screenshotsHtml = desktopImg
      ? `<div class="screenshots">
          <div class="screenshot-group">
            <span class="badge">Desktop</span>
            <img src="${desktopImg}" alt="${s.title} - Desktop" />
          </div>
          ${
            mobileImg
              ? `<div class="screenshot-group mobile">
              <span class="badge">Mobile</span>
              <img src="${mobileImg}" alt="${s.title} - Mobile" />
            </div>`
              : ''
          }
        </div>`
      : '<p class="no-screenshot"><em>Screenshot nao disponivel. Execute o script Playwright para gerar.</em></p>';

    sectionsHtml += `
    <section id="${s.id}">
      <h2>${s.title}</h2>
      <p class="description">${s.description}</p>
      <div class="features">
        <strong>Funcionalidades:</strong>
        <ul>
            ${featuresHtml}
        </ul>
      </div>
      ${tipsHtml}
      ${screenshotsHtml}
    </section>`;
  }

  return `<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>RVM.CodeLens - Manual do Usuario</title>
  <style>
    :root {
      --primary: #2f6fed;
      --surface: #ffffff;
      --bg: #f4f6fa;
      --text: #1e293b;
      --text-muted: #64748b;
      --border: #e2e8f0;
      --sidebar-bg: #0f172a;
      --accent: #3b82f6;
    }
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body {
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      background: var(--bg);
      color: var(--text);
      line-height: 1.6;
    }
    .container { max-width: 1100px; margin: 0 auto; padding: 2rem 1.5rem; }
    header { background: var(--sidebar-bg); color: white; padding: 3rem 1.5rem; text-align: center; }
    header h1 { font-size: 2rem; margin-bottom: 0.5rem; }
    header p { color: #94a3b8; font-size: 1rem; }
    header .version { color: #64748b; font-size: 0.85rem; margin-top: 0.5rem; }
    nav { background: var(--surface); border-bottom: 1px solid var(--border); padding: 1rem 1.5rem; position: sticky; top: 0; z-index: 100; }
    nav .container { padding: 0; }
    nav ul { list-style: none; display: flex; flex-wrap: wrap; gap: 0.5rem; }
    nav a { display: inline-block; padding: 0.35rem 0.75rem; border-radius: 0.5rem; font-size: 0.85rem; color: var(--text); text-decoration: none; background: var(--bg); transition: background 0.2s; }
    nav a:hover { background: var(--primary); color: white; }
    section { background: var(--surface); border: 1px solid var(--border); border-radius: 1rem; padding: 2rem; margin-bottom: 2rem; }
    section h2 { font-size: 1.5rem; color: var(--primary); margin-bottom: 1rem; padding-bottom: 0.5rem; border-bottom: 2px solid var(--border); }
    .description { font-size: 1.05rem; margin-bottom: 1.25rem; color: var(--text); }
    .features, .tips { background: var(--bg); border-radius: 0.75rem; padding: 1rem 1.25rem; margin-bottom: 1.25rem; }
    .features ul, .tips ul { margin-top: 0.5rem; padding-left: 1.25rem; }
    .features li, .tips li { margin-bottom: 0.35rem; }
    .tips { background: #eff6ff; border-left: 4px solid var(--accent); }
    .tips strong { color: var(--accent); }
    .screenshots { display: flex; gap: 1.5rem; margin-top: 1rem; align-items: flex-start; }
    .screenshot-group { position: relative; flex: 1; border: 1px solid var(--border); border-radius: 0.75rem; overflow: hidden; }
    .screenshot-group.mobile { flex: 0 0 200px; max-width: 200px; }
    .screenshot-group img { width: 100%; display: block; }
    .badge { position: absolute; top: 0.5rem; right: 0.5rem; background: var(--sidebar-bg); color: white; font-size: 0.7rem; padding: 0.2rem 0.5rem; border-radius: 0.35rem; font-weight: 600; text-transform: uppercase; }
    .no-screenshot { background: var(--bg); padding: 2rem; border-radius: 0.75rem; text-align: center; color: var(--text-muted); }
    footer { text-align: center; padding: 2rem 1rem; color: var(--text-muted); font-size: 0.85rem; }
    @media (max-width: 768px) { .screenshots { flex-direction: column; } .screenshot-group.mobile { max-width: 100%; flex: 1; } section { padding: 1.25rem; } }
    @media print { nav { display: none; } section { break-inside: avoid; page-break-inside: avoid; } .screenshots { flex-direction: column; } .screenshot-group.mobile { max-width: 250px; } }
  </style>
</head>
<body>
  <header>
    <h1>RVM.CodeLens - Manual do Usuario</h1>
    <p>Analise Estatica de Codigo com Roslyn — Guia Completo de Funcionalidades</p>
    <div class="version">Gerado em ${now} | RVM Tech</div>
  </header>

  <nav>
    <div class="container">
      <ul>
        ${sections.map((s) => `<li><a href="#${s.id}">${s.title}</a></li>`).join('\n        ')}
      </ul>
    </div>
  </nav>

  <div class="container">
    <section id="visao-geral">
      <h2>Visao Geral</h2>
      <p class="description">
        O <strong>RVM.CodeLens</strong> e uma plataforma de analise estatica de codigo .NET
        baseada em Roslyn. Fornece metricas de qualidade, visualizacao de dependencias,
        deteccao de hotspots de risco e analise de conformidade arquitetural.
      </p>
      <div class="features">
        <strong>Recursos principais:</strong>
        <ul>
          <li><strong>Analise Roslyn</strong> — metricas nativas do compilador .NET</li>
          <li><strong>Dashboard de qualidade</strong> — score, trends e issues por categoria</li>
          <li><strong>Grafico de dependencias</strong> — visualizacao interativa com deteccao de ciclos</li>
          <li><strong>Hotspots de risco</strong> — combinacao de complexidade x frequencia de mudancas</li>
          <li><strong>Visao arquitetural</strong> — conformidade com camadas e DDD</li>
          <li><strong>CLI + Web</strong> — analise via linha de comando ou interface grafica</li>
        </ul>
      </div>
    </section>

    ${sectionsHtml}
  </div>

  <footer>
    <p>RVM Tech &mdash; Analise Estatica de Codigo</p>
    <p>Documento gerado automaticamente com Playwright + TypeScript</p>
  </footer>
</body>
</html>`;
}

function generateMarkdown(): string {
  const now = new Date().toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });

  let md = `# RVM.CodeLens - Manual do Usuario

> Analise Estatica de Codigo com Roslyn — Guia Completo de Funcionalidades
>
> Gerado em ${now} | RVM Tech

---

## Visao Geral

O **RVM.CodeLens** e uma plataforma de analise estatica de codigo .NET baseada em Roslyn.

**Recursos principais:**
- **Analise Roslyn** — metricas nativas do compilador .NET
- **Dashboard de qualidade** — score, trends e issues por categoria
- **Grafico de dependencias** — visualizacao interativa com deteccao de ciclos
- **Hotspots de risco** — combinacao de complexidade x frequencia de mudancas
- **Visao arquitetural** — conformidade com camadas e DDD

---

`;

  for (const s of sections) {
    const desktopExists = fs.existsSync(path.join(SCREENSHOTS_DIR, `${s.screenshot}--desktop.png`));

    md += `## ${s.title}\n\n`;
    md += `${s.description}\n\n`;
    md += `**Funcionalidades:**\n`;
    for (const f of s.features) md += `- ${f}\n`;
    md += '\n';

    if (s.tips) {
      md += `> **Dicas:**\n`;
      for (const t of s.tips) md += `> - ${t}\n`;
      md += '\n';
    }

    if (desktopExists) {
      md += `| Desktop | Mobile |\n|---------|--------|\n`;
      md += `| ![${s.title} - Desktop](screenshots/${s.screenshot}--desktop.png) | ![${s.title} - Mobile](screenshots/${s.screenshot}--mobile.png) |\n`;
    } else {
      md += `*Screenshot nao disponivel. Execute o script Playwright para gerar.*\n`;
    }
    md += '\n---\n\n';
  }

  md += `## Informacoes Tecnicas

| Item | Detalhe |
|------|---------|
| **Tecnologia** | ASP.NET Core + Blazor Web |
| **Motor de analise** | Microsoft Roslyn (C# Compiler Platform) |
| **Banco de dados** | PostgreSQL 16 |
| **Visualizacao** | D3.js (grafos de dependencias) |

---

*Documento gerado automaticamente com Playwright + TypeScript — RVM Tech*
`;

  return md;
}

const html = generateHTML();
fs.writeFileSync(OUTPUT_HTML, html, 'utf-8');
console.log(`HTML gerado: ${OUTPUT_HTML}`);

const md = generateMarkdown();
fs.writeFileSync(OUTPUT_MD, md, 'utf-8');
console.log(`Markdown gerado: ${OUTPUT_MD}`);
