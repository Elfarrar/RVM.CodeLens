import { useEffect } from 'react';
import { useAnalysis } from '../../hooks/useAnalysis';

export default function ArchitecturePage() {
  const { analysis, loadCurrent } = useAnalysis();

  useEffect(() => { loadCurrent(); }, [loadCurrent]);

  if (!analysis) return <div className="empty-state">No analysis loaded.</div>;

  const { layers, violations } = analysis.architecture;

  return (
    <>
      <h1>Architecture</h1>

      {layers.map(layer => (
        <div key={layer.name} className="arch-layer">
          <h3>{layer.name}</h3>
          <div className="arch-projects">
            {layer.projects.map(p => <span key={p} className="arch-badge">{p}</span>)}
          </div>
        </div>
      ))}

      <h2>Violations</h2>
      {violations.length === 0 ? (
        <div className="success-box">No architecture violations detected.</div>
      ) : (
        violations.map((v, i) => <div key={i} className="violation-item">{v}</div>)
      )}
    </>
  );
}
