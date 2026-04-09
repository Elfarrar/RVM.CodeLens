import { useState, useCallback } from 'react';
import type { SolutionAnalysis } from '../types/models';
import { analyzeSolution, getCurrentAnalysis } from '../api/client';

export function useAnalysis() {
  const [analysis, setAnalysis] = useState<SolutionAnalysis | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const analyze = useCallback(async (path: string) => {
    setLoading(true);
    setError(null);
    try {
      const res = await analyzeSolution(path);
      setAnalysis(res.data);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Analysis failed');
    } finally {
      setLoading(false);
    }
  }, []);

  const loadCurrent = useCallback(async () => {
    try {
      const res = await getCurrentAnalysis();
      setAnalysis(res.data);
    } catch {
      // No current analysis
    }
  }, []);

  return { analysis, loading, error, analyze, loadCurrent };
}
