import { useEffect, useState } from 'react';
import type { HotSpot } from '../../types/models';
import { getHotSpots } from '../../api/client';
import { ScatterChart, Scatter, XAxis, YAxis, Tooltip, ResponsiveContainer, Cell } from 'recharts';

function scoreColor(score: number): string {
  if (score > 80) return '#f85149';
  if (score > 30) return '#d29922';
  return '#3fb950';
}

export default function HotSpotsPage() {
  const [hotSpots, setHotSpots] = useState<HotSpot[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getHotSpots()
      .then(res => setHotSpots(res.data))
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="loading">Analyzing git history...</div>;
  if (hotSpots.length === 0) return <div className="empty-state">No hot spots found.</div>;

  const chartData = hotSpots.slice(0, 30).map(h => ({
    commits: h.commitCount,
    complexity: h.complexity,
    score: h.score,
    file: h.filePath.split(/[/\\]/).pop() || h.filePath,
  }));

  return (
    <>
      <h1>Hot Spots</h1>
      <div className="chart-container">
        <ResponsiveContainer width="100%" height={300}>
          <ScatterChart>
            <XAxis dataKey="commits" name="Commits" tick={{ fill: '#8b949e' }} label={{ value: 'Commits (Churn)', position: 'bottom', fill: '#8b949e' }} />
            <YAxis dataKey="complexity" name="Complexity" tick={{ fill: '#8b949e' }} label={{ value: 'Complexity', angle: -90, position: 'left', fill: '#8b949e' }} />
            <Tooltip
              contentStyle={{ background: '#21262d', border: '1px solid #30363d', color: '#f0f6fc' }}
              formatter={(value: number, name: string) => [value.toFixed(1), name]}
              labelFormatter={(_, payload) => payload[0]?.payload?.file || ''}
            />
            <Scatter data={chartData}>
              {chartData.map((d, i) => <Cell key={i} fill={scoreColor(d.score)} />)}
            </Scatter>
          </ScatterChart>
        </ResponsiveContainer>
      </div>

      <table className="data-table">
        <thead><tr><th>File</th><th>Commits</th><th>Authors</th><th>Avg CC</th><th>Score</th></tr></thead>
        <tbody>
          {hotSpots.slice(0, 20).map((s, i) => (
            <tr key={i}>
              <td>{s.filePath}</td>
              <td>{s.commitCount}</td>
              <td>{s.authorCount}</td>
              <td>{s.complexity.toFixed(1)}</td>
              <td style={{ color: scoreColor(s.score), fontWeight: 600 }}>{s.score.toFixed(1)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </>
  );
}
