import { Routes, Route } from 'react-router-dom';
import AppLayout from './components/Layout/AppLayout';
import HomePage from './components/Dashboard/HomePage';
import DashboardPage from './components/Dashboard/DashboardPage';
import DependencyGraphPage from './components/DependencyGraph/DependencyGraphPage';
import MetricsPage from './components/Metrics/MetricsPage';
import HotSpotsPage from './components/HotSpots/HotSpotsPage';
import ArchitecturePage from './components/Architecture/ArchitecturePage';

export default function App() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/deps" element={<DependencyGraphPage />} />
        <Route path="/metrics" element={<MetricsPage />} />
        <Route path="/hotspots" element={<HotSpotsPage />} />
        <Route path="/architecture" element={<ArchitecturePage />} />
      </Route>
    </Routes>
  );
}
