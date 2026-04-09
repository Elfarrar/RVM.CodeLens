import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAnalysis } from '../../hooks/useAnalysis';

export default function HomePage() {
  const [path, setPath] = useState('');
  const { analysis, loading, error, analyze, loadCurrent } = useAnalysis();
  const navigate = useNavigate();

  const handleAnalyze = async () => {
    if (!path.trim()) return;
    await analyze(path);
    navigate('/dashboard');
  };

  return (
    <>
      <h1>CodeLens</h1>
      <p className="subtitle">Analyze your .NET solution</p>

      <label style={{ color: 'var(--text-secondary)' }}>Solution path (.sln or .slnx):</label>
      <div className="input-group">
        <input
          className="input-path"
          value={path}
          onChange={e => setPath(e.target.value)}
          placeholder="C:\path\to\solution.slnx"
          onKeyDown={e => e.key === 'Enter' && handleAnalyze()}
        />
        <button className="btn-primary" onClick={handleAnalyze} disabled={loading}>
          {loading ? 'Analyzing...' : 'Analyze'}
        </button>
      </div>
      {error && <p className="error-msg">{error}</p>}

      {analysis && (
        <div style={{ background: 'var(--bg-secondary)', border: '1px solid var(--border)', borderRadius: '8px', padding: '1.25rem', marginTop: '2rem' }}>
          <h3>Current Analysis</h3>
          <p><strong>{analysis.solutionName}</strong> — {analysis.projects.length} projects, {analysis.projects.reduce((s, p) => s + p.metrics.codeLines, 0).toLocaleString()} LOC</p>
          <a href="/dashboard" onClick={e => { e.preventDefault(); navigate('/dashboard'); }} style={{ color: 'var(--accent)', textDecoration: 'none', fontWeight: 500, display: 'inline-block', marginTop: '0.75rem' }}>
            Go to Dashboard &rarr;
          </a>
        </div>
      )}
    </>
  );
}
