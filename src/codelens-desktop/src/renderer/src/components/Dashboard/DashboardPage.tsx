import { useEffect } from 'react';
import { useAnalysis } from '../../hooks/useAnalysis';
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, Cell } from 'recharts';

function ccClass(cc: number): string {
  if (cc <= 5) return 'metric-good';
  if (cc <= 10) return 'metric-warn';
  return 'metric-bad';
}

function miClass(mi: number): string {
  if (mi >= 80) return 'metric-good';
  if (mi >= 50) return 'metric-warn';
  return 'metric-bad';
}

export default function DashboardPage() {
  const { analysis, loadCurrent } = useAnalysis();

  useEffect(() => { loadCurrent(); }, [loadCurrent]);

  if (!analysis) return <div className="empty-state">No analysis loaded. Go to Home to analyze a solution.</div>;

  const totalLoc = analysis.projects.reduce((s, p) => s + p.metrics.codeLines, 0);
  const totalFiles = analysis.projects.reduce((s, p) => s + p.metrics.fileCount, 0);
  const totalClasses = analysis.projects.reduce((s, p) => s + p.metrics.classCount, 0);
  const totalMethods = analysis.projects.reduce((s, p) => s + p.metrics.methodCount, 0);
  const avgCC = analysis.projects.length > 0
    ? analysis.projects.reduce((s, p) => s + p.metrics.averageCyclomaticComplexity, 0) / analysis.projects.length : 0;

  const chartData = analysis.projects.map(p => ({ name: p.name, loc: p.metrics.codeLines }));

  return (
    <>
      <h1>Dashboard</h1>
      <div className="stats-grid">
        <div className="stat-card"><div className="stat-value">{analysis.projects.length}</div><div className="stat-label">Projects</div></div>
        <div className="stat-card"><div className="stat-value">{totalLoc.toLocaleString()}</div><div className="stat-label">Lines of Code</div></div>
        <div className="stat-card"><div className="stat-value">{totalFiles}</div><div className="stat-label">Files</div></div>
        <div className="stat-card"><div className="stat-value">{totalClasses}</div><div className="stat-label">Classes</div></div>
        <div className="stat-card"><div className="stat-value">{totalMethods}</div><div className="stat-label">Methods</div></div>
        <div className={`stat-card ${ccClass(avgCC)}`}><div className="stat-value">{avgCC.toFixed(1)}</div><div className="stat-label">Avg Complexity</div></div>
      </div>

      <h2>Lines of Code by Project</h2>
      <div className="chart-container">
        <ResponsiveContainer width="100%" height={280}>
          <BarChart data={chartData}>
            <XAxis dataKey="name" tick={{ fill: '#8b949e', fontSize: 11 }} angle={-20} textAnchor="end" height={60} />
            <YAxis tick={{ fill: '#8b949e' }} />
            <Tooltip contentStyle={{ background: '#21262d', border: '1px solid #30363d', color: '#f0f6fc' }} />
            <Bar dataKey="loc" radius={[4, 4, 0, 0]}>
              {chartData.map((_, i) => <Cell key={i} fill="#58a6ff" />)}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </div>

      <h2>Projects</h2>
      <table className="data-table">
        <thead>
          <tr>
            <th>Project</th><th>Framework</th><th>Files</th><th>LOC</th>
            <th>Classes</th><th>Methods</th><th>Avg CC</th><th>Avg MI</th>
          </tr>
        </thead>
        <tbody>
          {analysis.projects.map(p => (
            <tr key={p.name}>
              <td>{p.name}</td>
              <td>{p.targetFramework}</td>
              <td>{p.metrics.fileCount}</td>
              <td>{p.metrics.codeLines.toLocaleString()}</td>
              <td>{p.metrics.classCount}</td>
              <td>{p.metrics.methodCount}</td>
              <td className={ccClass(p.metrics.averageCyclomaticComplexity)}>{p.metrics.averageCyclomaticComplexity.toFixed(1)}</td>
              <td className={miClass(p.metrics.averageMaintainabilityIndex)}>{p.metrics.averageMaintainabilityIndex.toFixed(1)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </>
  );
}
