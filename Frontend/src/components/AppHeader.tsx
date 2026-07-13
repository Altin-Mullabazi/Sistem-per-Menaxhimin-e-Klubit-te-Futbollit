import React, { useEffect } from 'react';
import { useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const routeTitles: Record<string, { title: string; group: string }> = {
  '/dashboard': { title: 'Dashboard', group: 'Overview' },
  '/clubs': { title: 'Clubs', group: 'Club Data' },
  '/players': { title: 'Players', group: 'Club Data' },
  '/stadiums': { title: 'Stadiums', group: 'Club Data' },
  '/matches': { title: 'Matches', group: 'Competition' },
  '/player-stats': { title: 'Player Statistics', group: 'Competition' },
  '/seasons': { title: 'Seasons', group: 'Operations' },
  '/training-sessions': { title: 'Training Sessions', group: 'Operations' },
  '/staff': { title: 'Staff', group: 'Operations' },
  '/transfers': { title: 'Transfers', group: 'Management' },
  '/contracts': { title: 'Contracts', group: 'Management' },
  '/injuries': { title: 'Injuries', group: 'Management' },
  '/management': { title: 'Management Hub', group: 'Management' },
  '/users': { title: 'Users & Access', group: 'Administration' },
  '/sponsors-seasons': { title: 'Sponsors & Seasons', group: 'Administration' },
  '/profile': { title: 'My Profile', group: 'Account' },
};

export const AppHeader: React.FC = () => {
  const { isAuthenticated, user } = useAuth();
  const { pathname } = useLocation();
  const current = routeTitles[pathname] ?? { title: 'SquadHQ', group: 'Workspace' };

  useEffect(() => {
    document.title = `${current.title} | SquadHQ`;
  }, [current.title]);

  if (!isAuthenticated) return null;

  return (
    <header className="app-context-header">
      <div>
        <div className="breadcrumbs" aria-label="Breadcrumb">
          <span>SquadHQ</span>
          <span aria-hidden="true">/</span>
          <span>{current.group}</span>
        </div>
        <h1>{current.title}</h1>
      </div>
      <div className="context-user">
        <span className="context-status"><span /> System online</span>
        <div className="context-avatar">{(user?.username || 'U').charAt(0).toUpperCase()}</div>
        <div>
          <strong>{user?.username}</strong>
          <span>{user?.role || 'Member'}</span>
        </div>
      </div>
    </header>
  );
};
