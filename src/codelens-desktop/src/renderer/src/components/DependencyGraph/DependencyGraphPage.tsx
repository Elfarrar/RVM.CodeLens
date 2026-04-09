import { useEffect, useState } from 'react';
import { useAnalysis } from '../../hooks/useAnalysis';
import ForceGraph from './ForceGraph';

export default function DependencyGraphPage() {
  const { analysis, loadCurrent } = useAnalysis();
  const [showPackages, setShowPackages] = useState(false);

  useEffect(() => { loadCurrent(); }, [loadCurrent]);

  if (!analysis) return <div className="empty-state">No analysis loaded.</div>;

  const { nodes, edges } = analysis.dependencyGraph;
  const filteredNodes = showPackages ? nodes : nodes.filter(n => n.type === 'project');
  const nodeIds = new Set(filteredNodes.map(n => n.id));
  const filteredEdges = edges.filter(e => nodeIds.has(e.source) && nodeIds.has(e.target));

  return (
    <>
      <h1>Dependency Graph</h1>
      <label style={{ color: 'var(--text-secondary)', cursor: 'pointer' }}>
        <input type="checkbox" checked={showPackages} onChange={e => setShowPackages(e.target.checked)} />
        {' '}Show NuGet Packages
      </label>
      <div className="graph-container">
        <ForceGraph nodes={filteredNodes} edges={filteredEdges} />
      </div>
    </>
  );
}
