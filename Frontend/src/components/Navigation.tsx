import React, { useEffect, useState } from 'react';
import { NavLink, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Icon, IconName } from './Icon';
import '../styles/Navigation.css';

interface NavigationItem {
  label: string;
  path: string;
  icon: IconName;
  adminOnly?: boolean;
  manageOnly?: boolean;
}

interface NavigationGroup {
  label: string;
  items: NavigationItem[];
}

const groups: NavigationGroup[] = [
  {
    label: 'Overview',
    items: [{ label: 'Dashboard', path: '/dashboard', icon: 'dashboard' }],
  },
  {
    label: 'Club Data',
    items: [
      { label: 'Clubs', path: '/clubs', icon: 'building' },
      { label: 'Players', path: '/players', icon: 'players' },
      { label: 'Stadiums', path: '/stadiums', icon: 'stadium' },
    ],
  },
  {
    label: 'Competition',
    items: [
      { label: 'Matches', path: '/matches', icon: 'activity' },
      { label: 'Player Stats', path: '/player-stats', icon: 'stats' },
    ],
  },
  {
    label: 'Operations',
    items: [
      { label: 'Seasons', path: '/seasons', icon: 'calendar' },
      { label: 'Training', path: '/training-sessions', icon: 'training' },
      { label: 'Staff', path: '/staff', icon: 'users' },
    ],
  },
  {
    label: 'Management',
    items: [
      { label: 'Transfers', path: '/transfers', icon: 'transfer', manageOnly: true },
      { label: 'Contracts', path: '/contracts', icon: 'contract', manageOnly: true },
      { label: 'Injuries', path: '/injuries', icon: 'injury', manageOnly: true },
      { label: 'Management Hub', path: '/management', icon: 'settings', manageOnly: true },
    ],
  },
  {
    label: 'Administration',
    items: [
      { label: 'Users & Roles', path: '/users', icon: 'shield', adminOnly: true },
      { label: 'Sponsors', path: '/sponsors-seasons', icon: 'clipboard', adminOnly: true },
    ],
  },
];

export const Navigation: React.FC = () => {
  const { user, logout, isAuthenticated, isAdmin, canManage } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [isCollapsed, setIsCollapsed] = useState(() => localStorage.getItem('sidebarCollapsed') === 'true');
  const [isMobileOpen, setIsMobileOpen] = useState(false);

  useEffect(() => {
    setIsMobileOpen(false);
  }, [location.pathname]);

  useEffect(() => {
    localStorage.setItem('sidebarCollapsed', String(isCollapsed));
  }, [isCollapsed]);

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  if (!isAuthenticated || !user) return null;

  const visibleGroups = groups
    .map((group) => ({
      ...group,
      items: group.items.filter((item) => {
        if (item.adminOnly) return isAdmin;
        if (item.manageOnly) return canManage;
        return true;
      }),
    }))
    .filter((group) => group.items.length > 0);

  return (
    <>
      <header className="mobile-app-bar">
        <button className="mobile-brand" onClick={() => navigate('/dashboard')}>
          <span className="brand-mark">FC</span>
          <span>Club Command</span>
        </button>
        <button
          className="mobile-menu-trigger"
          onClick={() => setIsMobileOpen(true)}
          aria-label="Open navigation"
          aria-expanded={isMobileOpen}
        >
          <Icon name="menu" />
        </button>
      </header>

      {isMobileOpen && (
        <button
          className="sidebar-backdrop"
          onClick={() => setIsMobileOpen(false)}
          aria-label="Close navigation"
        />
      )}

      <aside className={`sidebar ${isCollapsed ? 'is-collapsed' : ''} ${isMobileOpen ? 'is-mobile-open' : ''}`}>
        <div className="sidebar-brand">
          <button className="brand-button" onClick={() => navigate('/dashboard')} title="Club Command">
            <span className="brand-mark">FC</span>
            <span className="brand-copy">
              <strong>Club Command</strong>
              <small>Football operations</small>
            </span>
          </button>
          <button
            className="sidebar-mobile-close"
            onClick={() => setIsMobileOpen(false)}
            aria-label="Close navigation"
          >
            <Icon name="x" />
          </button>
        </div>

        <nav className="sidebar-navigation" aria-label="Main navigation">
          {visibleGroups.map((group) => (
            <div className="sidebar-group" key={group.label}>
              <div className="sidebar-group-label">{group.label}</div>
              {group.items.map((item) => (
                <NavLink
                  key={item.path}
                  to={item.path}
                  className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
                  title={isCollapsed ? item.label : undefined}
                >
                  <Icon name={item.icon} />
                  <span>{item.label}</span>
                  <Icon name="chevron" size={15} className="link-chevron" />
                </NavLink>
              ))}
            </div>
          ))}
        </nav>

        <div className="sidebar-footer">
          <NavLink
            to="/profile"
            className={({ isActive }) => `sidebar-profile ${isActive ? 'active' : ''}`}
            title={isCollapsed ? 'Profile' : undefined}
          >
            <span className="sidebar-avatar">{user.username.charAt(0).toUpperCase()}</span>
            <span className="sidebar-profile-copy">
              <strong>{user.username}</strong>
              <small>{user.role || 'Member'}</small>
            </span>
          </NavLink>
          <button className="sidebar-logout" onClick={handleLogout} title="Logout">
            <Icon name="logout" />
            <span>Logout</span>
          </button>
          <button
            className="sidebar-collapse"
            onClick={() => setIsCollapsed((current) => !current)}
            aria-label={isCollapsed ? 'Expand sidebar' : 'Collapse sidebar'}
          >
            <Icon name="chevron" />
            <span>Collapse menu</span>
          </button>
        </div>
      </aside>
    </>
  );
};
