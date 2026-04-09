import axios from 'axios';
import type {
  SolutionAnalysis,
  DependencyGraph,
  HotSpot,
  ArchitectureAnalysis,
} from '../types/models';

const api = axios.create({
  baseURL: 'http://localhost:5000/api',
  timeout: 120000,
});

export const analyzeSolution = (solutionPath: string) =>
  api.post<SolutionAnalysis>('/analyze', { solutionPath });

export const getCurrentAnalysis = () =>
  api.get<SolutionAnalysis>('/analysis/current');

export const getMetrics = () =>
  api.get('/metrics');

export const getDependencyGraph = () =>
  api.get<DependencyGraph>('/deps');

export const getHotSpots = () =>
  api.get<HotSpot[]>('/hotspots');

export const getArchitecture = () =>
  api.get<ArchitectureAnalysis>('/architecture');

export const checkHealth = () =>
  api.get('/health');
