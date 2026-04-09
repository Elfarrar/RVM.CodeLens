import { NavLink } from 'react-router-dom';

const links = [
  { to: '/', label: 'Home' },
  { to: '/dashboard', label: 'Dashboard' },
  { to: '/deps', label: 'Dependencies' },
  { to: '/metrics', label: 'Metrics' },
  { to: '/hotspots', label: 'Hot Spots' },
  { to: '/architecture', label: 'Architecture' },
];

export default function Sidebar() {
  return (
    <nav className="sidebar">
      <h2>CodeLens</h2>
      <ul className="nav-menu">
        {links.map(({ to, label }) => (
          <li key={to}>
            <NavLink to={to} className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
              {label}
            </NavLink>
          </li>
        ))}
      </ul>
    </nav>
  );
}
