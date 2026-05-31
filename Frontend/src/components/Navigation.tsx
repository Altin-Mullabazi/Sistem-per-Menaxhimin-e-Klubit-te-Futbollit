import React, { useState, useRef, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { NavLink, useNavigate, useLocation } from 'react-router-dom';
import '../styles/Navigation.css';

export const Navigation: React.FC = () => {
  const { user, logout, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [activeDropdown, setActiveDropdown] = useState<string | null>(null);
  const [userMenuOpen, setUserMenuOpen] = useState(false);
  const userMenuRef = useRef<HTMLDivElement>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);

  // Close menus when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (userMenuRef.current && !userMenuRef.current.contains(event.target as Node)) {
        setUserMenuOpen(false);
      }
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setActiveDropdown(null);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  // Close mobile menu on route change
  useEffect(() => {
    setMobileMenuOpen(false);
    setActiveDropdown(null);
  }, [location.pathname]);

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  const toggleDropdown = (name: string) => {
    setActiveDropdown(activeDropdown === name ? null : name);
  };

  const handleNavClick = (path: string) => {
    navigate(path);
    setMobileMenuOpen(false);
    setActiveDropdown(null);
  };

  const isAdmin = user?.role === 'Admin';
  const isManager = user?.role === 'Manager';

  return (
    <nav className="navbar">
      <div className="navbar-container">
        {/* Brand */}
        <div className="navbar-brand">
          <button 
            className="navbar-brand-btn"
            onClick={() => {
              navigate('/dashboard');
              setMobileMenuOpen(false);
            }}
          >
            <span className="brand-icon">⚽</span>
            <span className="brand-text">Football Club</span>
          </button>
        </div>

        {isAuthenticated && user && (
          <>
            {/* Hamburger Menu Button */}
            <button 
              className={`hamburger-btn ${mobileMenuOpen ? 'active' : ''}`}
              onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
              aria-label="Toggle menu"
            >
              <span></span>
              <span></span>
              <span></span>
            </button>

            {/* Desktop Menu */}
            <div className="navbar-menu-desktop">
              <div className="navbar-links" ref={dropdownRef}>
                {/* Main Links */}
                <NavLink 
                  to="/dashboard" 
                  className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
                >
                  📊 Dashboard
                </NavLink>

                {/* Manage Data Dropdown */}
                <div className="dropdown-menu">
                  <button 
                    className={`nav-link dropdown-toggle ${activeDropdown === 'manage' ? 'active' : ''}`}
                    onClick={() => toggleDropdown('manage')}
                  >
                    📋 Manage Data <span className="dropdown-icon">▾</span>
                  </button>
                  {activeDropdown === 'manage' && (
                    <div className="dropdown-content">
                      <NavLink to="/clubs" className="dropdown-link">Clubs</NavLink>
                      <NavLink to="/players" className="dropdown-link">Players</NavLink>
                      <NavLink to="/stadiums" className="dropdown-link">Stadiums</NavLink>
                    </div>
                  )}
                </div>

                {/* Match System Dropdown */}
                <div className="dropdown-menu">
                  <button 
                    className={`nav-link dropdown-toggle ${activeDropdown === 'matches' ? 'active' : ''}`}
                    onClick={() => toggleDropdown('matches')}
                  >
                    ⚔️ Match System <span className="dropdown-icon">▾</span>
                  </button>
                  {activeDropdown === 'matches' && (
                    <div className="dropdown-content">
                      <NavLink to="/matches" className="dropdown-link">Matches</NavLink>
                      <NavLink to="/player-stats" className="dropdown-link">Events & Stats</NavLink>
                    </div>
                  )}
                </div>

                {/* Management Dropdown - Manager and Admin only */}
                {(isAdmin || isManager) && (
                  <div className="dropdown-menu">
                    <button 
                      className={`nav-link dropdown-toggle ${activeDropdown === 'management' ? 'active' : ''}`}
                      onClick={() => toggleDropdown('management')}
                    >
                      ⚙️ Management <span className="dropdown-icon">▾</span>
                    </button>
                    {activeDropdown === 'management' && (
                      <div className="dropdown-content">
                        <NavLink to="/transfers" className="dropdown-link">Transfers</NavLink>
                        <NavLink to="/contracts" className="dropdown-link">Contracts</NavLink>
                        <NavLink to="/injuries" className="dropdown-link">Injuries</NavLink>
                      </div>
                    )}
                  </div>
                )}

                {/* Admin Dropdown - Admin only */}
                {isAdmin && (
                  <div className="dropdown-menu">
                    <button 
                      className={`nav-link dropdown-toggle ${activeDropdown === 'admin' ? 'active' : ''}`}
                      onClick={() => toggleDropdown('admin')}
                    >
                      👑 Admin <span className="dropdown-icon">▾</span>
                    </button>
                    {activeDropdown === 'admin' && (
                      <div className="dropdown-content">
                        <NavLink to="/users" className="dropdown-link">Users</NavLink>
                        <NavLink to="/sponsors-seasons" className="dropdown-link">Sponsors & Seasons</NavLink>
                      </div>
                    )}
                  </div>
                )}
              </div>

              {/* User Menu */}
              <div className="user-menu-desktop" ref={userMenuRef}>
                <button 
                  className="user-menu-btn"
                  onClick={() => setUserMenuOpen(!userMenuOpen)}
                >
                  <span className="user-avatar">👤</span>
                  <div className="user-info">
                    <div className="user-name">{user.username}</div>
                    <div className="user-role">{user.role || 'Fan'}</div>
                  </div>
                  <span className="user-menu-icon">▾</span>
                </button>
                {userMenuOpen && (
                  <div className="user-menu-dropdown">
                    <button className="user-menu-item" onClick={() => navigate('/profile')}>
                      👤 Profile
                    </button>
                    <div className="user-menu-divider"></div>
                    <button className="user-menu-item logout" onClick={handleLogout}>
                      🚪 Logout
                    </button>
                  </div>
                )}
              </div>
            </div>

            {/* Mobile Menu */}
            {mobileMenuOpen && (
              <div className="navbar-menu-mobile">
                <div className="mobile-menu-content">
                  {/* Close Button */}
                  <button 
                    className="mobile-menu-close"
                    onClick={() => setMobileMenuOpen(false)}
                  >
                    ✕
                  </button>

                  {/* Mobile Links */}
                  <div className="mobile-links">
                    <button 
                      className="mobile-link"
                      onClick={() => handleNavClick('/dashboard')}
                    >
                      📊 Dashboard
                    </button>

                    {/* Manage Data Mobile */}
                    <div className="mobile-dropdown">
                      <button 
                        className={`mobile-link ${activeDropdown === 'manage-mobile' ? 'active' : ''}`}
                        onClick={() => setActiveDropdown(activeDropdown === 'manage-mobile' ? null : 'manage-mobile')}
                      >
                        📋 Manage Data <span>▾</span>
                      </button>
                      {activeDropdown === 'manage-mobile' && (
                        <div className="mobile-dropdown-content">
                          <button 
                            className="mobile-dropdown-link"
                            onClick={() => handleNavClick('/clubs')}
                          >
                            Clubs
                          </button>
                          <button 
                            className="mobile-dropdown-link"
                            onClick={() => handleNavClick('/players')}
                          >
                            Players
                          </button>
                          <button 
                            className="mobile-dropdown-link"
                            onClick={() => handleNavClick('/stadiums')}
                          >
                            Stadiums
                          </button>
                        </div>
                      )}
                    </div>

                    {/* Match System Mobile */}
                    <div className="mobile-dropdown">
                      <button 
                        className={`mobile-link ${activeDropdown === 'matches-mobile' ? 'active' : ''}`}
                        onClick={() => setActiveDropdown(activeDropdown === 'matches-mobile' ? null : 'matches-mobile')}
                      >
                        ⚔️ Match System <span>▾</span>
                      </button>
                      {activeDropdown === 'matches-mobile' && (
                        <div className="mobile-dropdown-content">
                          <button 
                            className="mobile-dropdown-link"
                            onClick={() => handleNavClick('/matches')}
                          >
                            Matches
                          </button>
                          <button 
                            className="mobile-dropdown-link"
                            onClick={() => handleNavClick('/player-stats')}
                          >
                            Events & Stats
                          </button>
                        </div>
                      )}
                    </div>

                    {/* Management Mobile - Manager and Admin only */}
                    {(isAdmin || isManager) && (
                      <div className="mobile-dropdown">
                        <button 
                          className={`mobile-link ${activeDropdown === 'management-mobile' ? 'active' : ''}`}
                          onClick={() => setActiveDropdown(activeDropdown === 'management-mobile' ? null : 'management-mobile')}
                        >
                          ⚙️ Management <span>▾</span>
                        </button>
                        {activeDropdown === 'management-mobile' && (
                          <div className="mobile-dropdown-content">
                            <button 
                              className="mobile-dropdown-link"
                              onClick={() => handleNavClick('/transfers')}
                            >
                              Transfers
                            </button>
                            <button 
                              className="mobile-dropdown-link"
                              onClick={() => handleNavClick('/contracts')}
                            >
                              Contracts
                            </button>
                            <button 
                              className="mobile-dropdown-link"
                              onClick={() => handleNavClick('/injuries')}
                            >
                              Injuries
                            </button>
                          </div>
                        )}
                      </div>
                    )}

                    {/* Admin Mobile - Admin only */}
                    {isAdmin && (
                      <div className="mobile-dropdown">
                        <button 
                          className={`mobile-link ${activeDropdown === 'admin-mobile' ? 'active' : ''}`}
                          onClick={() => setActiveDropdown(activeDropdown === 'admin-mobile' ? null : 'admin-mobile')}
                        >
                          👑 Admin <span>▾</span>
                        </button>
                        {activeDropdown === 'admin-mobile' && (
                          <div className="mobile-dropdown-content">
                            <button 
                              className="mobile-dropdown-link"
                              onClick={() => handleNavClick('/users')}
                            >
                              Users
                            </button>
                            <button 
                              className="mobile-dropdown-link"
                              onClick={() => handleNavClick('/sponsors-seasons')}
                            >
                              Sponsors & Seasons
                            </button>
                          </div>
                        )}
                      </div>
                    )}
                  </div>

                  {/* Mobile User Menu */}
                  <div className="mobile-user-section">
                    <div className="mobile-user-info">
                      <div className="mobile-user-name">{user.username}</div>
                      <div className="mobile-user-role">{user.role || 'Fan'}</div>
                    </div>
                    <button 
                      className="mobile-link logout"
                      onClick={handleLogout}
                    >
                      🚪 Logout
                    </button>
                  </div>
                </div>
              </div>
            )}
          </>
        )}
      </div>
    </nav>
  );
};
