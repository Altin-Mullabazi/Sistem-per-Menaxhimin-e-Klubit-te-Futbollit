import React, { createContext, useContext, useState, useCallback, useEffect } from 'react';
import { User, AuthResponse } from '../types';
import { authService } from '../services/authService';

interface AuthContextType {
  user: User | null;
  accessToken: string | null;
  isLoading: boolean;
  error: string | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (username: string, email: string, password: string, role: string) => Promise<void>;
  logout: () => Promise<void>;
  clearError: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [accessToken, setAccessToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Handle token refresh from apiClient interceptor
  useEffect(() => {
    const handleTokenRefreshed = (event: Event) => {
      const customEvent = event as CustomEvent;
      setAccessToken(customEvent.detail.accessToken);
    };

    const handleTokenExpired = () => {
      void logout();
    };

    window.addEventListener('tokenRefreshed', handleTokenRefreshed);
    window.addEventListener('tokenExpired', handleTokenExpired);

    return () => {
      window.removeEventListener('tokenRefreshed', handleTokenRefreshed);
      window.removeEventListener('tokenExpired', handleTokenExpired);
    };
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const response: AuthResponse = await authService.login({ email, password });
      if (response.success) {
        setAccessToken(response.accessToken || null);
        setUser(response.user || null);
      } else {
        setError(response.message);
      }
    } catch (err: any) {
      setError(err.message || 'Login failed');
    } finally {
      setIsLoading(false);
    }
  }, []);

  const register = useCallback(async (username: string, email: string, password: string, role: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const response: AuthResponse = await authService.register({ username, email, password, role });
      if (response.success) {
        setAccessToken(response.accessToken || null);
        setUser(response.user || null);
      } else {
        setError(response.message);
      }
    } catch (err: any) {
      setError(err.message || 'Registration failed');
    } finally {
      setIsLoading(false);
    }
  }, []);

  const logout = useCallback(async () => {
    await authService.logout();
    setUser(null);
    setAccessToken(null);
    setError(null);
  }, []);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  const value: AuthContextType = {
    user,
    accessToken,
    isLoading,
    error,
    isAuthenticated: !!accessToken && !!user,
    login,
    register,
    logout,
    clearError,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
