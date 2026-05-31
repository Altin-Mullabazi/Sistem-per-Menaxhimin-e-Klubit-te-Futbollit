import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { Navigation } from './components/Navigation';
import { Login } from './pages/Login';
import Dashboard from './pages/Dashboard';
import Players from './pages/Players';
import SponsorsSeasons from './pages/SponsorsSeasons';
import Management from './pages/Management';
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
                path="/players"
                element={
                  <ProtectedRoute>
                    <Players />
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
                path="/management"
                element={
                  <ProtectedRoute>
                    <Management />
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
