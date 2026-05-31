import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { Navigation } from './components/Navigation';
import { Login } from './pages/Login';
import Dashboard from './pages/Dashboard';
import Clubs from './pages/Clubs';
import Players from './pages/Players';
import Matches from './pages/Matches';
import PlayerStatsPage from './pages/PlayerStatsPage';
import UsersPage from './pages/UsersPage';
import SponsorsSeasons from './pages/SponsorsSeasons';
import Transfers from './pages/Transfers';
import Contracts from './pages/Contracts';
import Injuries from './pages/Injuries';
import Stadiums from './pages/Stadiums';
import Profile from './pages/Profile';
import './styles/App.css';

function App() {
  return (
    <Router>
      <AuthProvider>
        <div className="app">
          <Navigation />
          <main className="main-content">
            <Routes>
              <Route path="/login" element={<Login />} />

              <Route
                path="/dashboard"
                element={
                  <ProtectedRoute>
                    <Dashboard />
                  </ProtectedRoute>
                }
              />

              <Route
                path="/clubs"
                element={
                  <ProtectedRoute>
                    <Clubs />
                  </ProtectedRoute>
                }
              />

              <Route
                path="/players"
                element={
                  <ProtectedRoute>
                    <Players />
                  </ProtectedRoute>
                }
              />

              <Route
                path="/matches"
                element={
                  <ProtectedRoute>
                    <Matches />
                  </ProtectedRoute>
                }
              />

              <Route
                path="/player-stats"
                element={
                  <ProtectedRoute>
                    <PlayerStatsPage />
                  </ProtectedRoute>
                }
              />

              <Route
                path="/transfers"
                element={
                  <ProtectedRoute>
                    <Transfers />
                  </ProtectedRoute>
                }
              />

              <Route
                path="/contracts"
                element={
                  <ProtectedRoute>
                    <Contracts />
                  </ProtectedRoute>
                }
              />

              <Route
                path="/injuries"
                element={
                  <ProtectedRoute>
                    <Injuries />
                  </ProtectedRoute>
                }
              />

              <Route
                path="/stadiums"
                element={
                  <ProtectedRoute>
                    <Stadiums />
                  </ProtectedRoute>
                }
              />

              <Route
                path="/users"
                element={
                  <ProtectedRoute>
                    <UsersPage />
                  </ProtectedRoute>
                }
              />

              <Route
                path="/sponsors-seasons"
                element={
                  <ProtectedRoute>
                    <SponsorsSeasons />
                  </ProtectedRoute>
                }
              />

              <Route
                path="/profile"
                element={
                  <ProtectedRoute>
                    <Profile />
                  </ProtectedRoute>
                }
              />

              <Route path="/" element={<Navigate to="/dashboard" replace />} />
              <Route path="*" element={<NotFoundPage />} />
            </Routes>
          </main>
        </div>
      </AuthProvider>
    </Router>
  );
}

const NotFoundPage: React.FC = () => (
  <div className="error-page">
    <h1>404 - Page Not Found</h1>
    <p>The page you're looking for doesn't exist.</p>
  </div>
);

export default App;
