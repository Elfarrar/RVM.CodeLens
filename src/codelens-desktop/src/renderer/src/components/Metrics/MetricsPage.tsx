import { useEffect } from 'react';
import { useAnalysis } from '../../hooks/useAnalysis';

function ccClass(cc: number): string {
  if (cc <= 5) return 'metric-good';
  if (cc <= 10) return 'metric-warn';
  return 'metric-bad';
}

export default function MetricsPage() {
  const { analysis, loadCurrent } = useAnalysis();

  useEffect(() => { loadCurrent(); }, [loadCurrent]);

  if (!analysis) return <div className="empty-state">No analysis loaded.</div>;

  return (
    <>
      <h1>Code Metrics</h1>
      {analysis.projects.map(project => {
        const complexMethods = project.metrics.files
          .flatMap(f => f.types)
          .flatMap(t => t.methods.map(m => ({ typeName: t.name, method: m })))
          .filter(x => x.method.cyclomaticComplexity > 3)
          .sort((a, b) => b.method.cyclomaticComplexity - a.method.cyclomaticComplexity)
          .slice(0, 15);

        return (
          <div key={project.name} style={{ marginBottom: '2rem', paddingBottom: '1.5rem', borderBottom: '1px solid var(--border)' }}>
            <h2>{project.name}</h2>
            <p style={{ color: 'var(--text-secondary)', fontSize: '0.875rem', marginBottom: '0.75rem' }}>
              {project.metrics.fileCount} files | {project.metrics.codeLines.toLocaleString()} LOC |
              {project.metrics.classCount} classes | {project.metrics.methodCount} methods
            </p>

            {complexMethods.length > 0 ? (
              <>
                <h3 style={{ fontSize: '0.9rem', margin: '0.75rem 0 0.5rem' }}>Complex Methods (CC &gt; 3)</h3>
                <table className="data-table">
                  <thead>
                    <tr><th>Type</th><th>Method</th><th>Lines</th><th>CC</th><th>Coupling</th></tr>
                  </thead>
                  <tbody>
                    {complexMethods.map((m, i) => (
                      <tr key={i}>
                        <td>{m.typeName}</td>
                        <td>{m.method.name}</td>
                        <td>{m.method.lineCount}</td>
                        <td className={ccClass(m.method.cyclomaticComplexity)}>{m.method.cyclomaticComplexity}</td>
                        <td>{m.method.classCoupling}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </>
            ) : (
              <p style={{ color: 'var(--text-secondary)' }}>All methods have low complexity.</p>
            )}
          </div>
        );
      })}
    </>
  );
}
