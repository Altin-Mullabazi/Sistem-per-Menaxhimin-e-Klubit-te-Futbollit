import React from 'react';
import { useAuth } from '../context/AuthContext';
import { NavLink, useNavigate } from 'react-router-dom';
import '../styles/Navigation.css';

export const Navigation: React.FC = () => {
  const { user, logout, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <nav className="navbar">
      <div className="navbar-container">
        <div className="navbar-brand">
          <h1>⚽ Football Club</h1>
        </div>

        {isAuthenticated && user && (
          <>
            <div className="navbar-links">
              <NavLink to="/dashboard" className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}>
                Dashboard
              </NavLink>
              <NavLink to="/players" className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}>
                Players
              </NavLink>
              <NavLink to="/matches" className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}>
                Matches
              </NavLink>
              <NavLink to="/seasons" className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}>
                Seasons
              </NavLink>
              <NavLink to="/staff" className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}>
                Staff
              </NavLink>
              <NavLink to="/training-sessions" className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}>
                Training
              </NavLink>
              <NavLink to="/player-stats" className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}>
                Player Stats
              </NavLink>
              {user?.role === 'Admin' && (
                <NavLink to="/users" className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}>
                  Users
                </NavLink>
              )}
              <NavLink to="/sponsors-seasons" className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}>
                Sponsors & Seasons
              </NavLink>
            </div>
            <div className="navbar-content">
              <div className="navbar-user-info">
                <span className="user-name">{user.username}</span>
                <span className="user-role">{user.role || 'Fan'}</span>
              </div>
              <button className="btn btn-logout" onClick={handleLogout}>
                Logout
              </button>
            </div>
          </>
        )}
      </div>
    </nav>
  );
};
