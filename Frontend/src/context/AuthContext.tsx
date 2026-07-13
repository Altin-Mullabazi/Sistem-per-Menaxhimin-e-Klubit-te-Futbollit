import React, { createContext, useContext, useState, useCallback, useEffect } from 'react';
import { User, AuthResponse } from '../types';
import { authService } from '../services/authService';
import { setAuthTokens, clearAuthTokens } from '../services/apiClient';

export const normalizeRole = (role?: string): string => (role ?? '').trim().toLowerCase();

export const hasRole = (role: string | undefined, ...allowed: string[]): boolean =>
  allowed.some((r) => normalizeRole(role) === r.toLowerCase());

const normalizeUser = (raw: Record<string, unknown> | null | undefined): User | null => {
  if (!raw) {
    return null;
  }

  const role = (raw.role ?? raw.Role) as string | undefined;

  return {
    id: String(raw.id ?? raw.Id ?? ''),
    username: String(raw.username ?? raw.Username ?? raw.email ?? raw.Email ?? ''),
    email: String(raw.email ?? raw.Email ?? ''),
    role: role ? String(role) : undefined,
    firstName: (raw.firstName ?? raw.FirstName) as string | undefined,
    lastName: (raw.lastName ?? raw.LastName) as string | undefined,
    fullName: (raw.fullName ?? raw.FullName) as string | undefined,
    createdAt: (raw.createdAt ?? raw.CreatedAt) as string | undefined,
    updatedAt: (raw.updatedAt ?? raw.UpdatedAt) as string | undefined,
  };
};

const parseStoredUser = (stored: string): User | null => {
  try {
    return normalizeUser(JSON.parse(stored) as Record<string, unknown>);
  } catch {
    return null;
  }
};

interface AuthContextType {
  user: User | null;
  accessToken: string | null;
  isLoading: boolean;
  error: string | null;
  isAuthenticated: boolean;
  isAdmin: boolean;
  isManager: boolean;
  canManage: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (firstName: string, lastName: string, email: string, password: string, confirmPassword: string) => Promise<void>;
  logout: () => Promise<void>;
  clearError: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(() => {
    const stored = localStorage.getItem('user');
    return stored ? parseStoredUser(stored) : null;
  });

  const [accessToken, setAccessToken] = useState<string | null>(() => {
    return localStorage.getItem('accessToken');
  });
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

  // Restore auth state from localStorage on mount
  useEffect(() => {
    const storedAccess = localStorage.getItem('accessToken');
    const storedRefresh = localStorage.getItem('refreshToken');
    const storedUser = localStorage.getItem('user');

    if (storedAccess) {
      setAccessToken(storedAccess);
      setAuthTokens(storedAccess, storedRefresh || '');
    }

    if (storedUser) {
      const parsed = parseStoredUser(storedUser);
      if (parsed) {
        setUser(parsed);
      }
    }
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const response: AuthResponse = await authService.login({ email, password });
      if (response.success) {
        setAccessToken(response.accessToken || null);
        const normalizedUser = normalizeUser(
          (response.user ?? null) as unknown as Record<string, unknown> | null
        );
        setUser(normalizedUser);

        if (response.accessToken) {
          localStorage.setItem('accessToken', response.accessToken);
        }
        if (response.refreshToken) {
          localStorage.setItem('refreshToken', response.refreshToken);
        }
        if (normalizedUser) {
          localStorage.setItem('user', JSON.stringify(normalizedUser));
        }
        // ensure apiClient has tokens
        if (response.accessToken || response.refreshToken) {
          setAuthTokens(response.accessToken || '', response.refreshToken || '');
        }
      } else {
        const message = response.message || 'Login failed';
        setError(message);
        throw new Error(message);
      }
    } catch (err: any) {
      const message =
        err?.message ||
        err?.Message ||
        (typeof err === 'string' ? err : null) ||
        'Login failed';
      setError(message);
    } finally {
      setIsLoading(false);
    }
  }, []);

  const register = useCallback(async (firstName: string, lastName: string, email: string, password: string, confirmPassword: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const response: AuthResponse = await authService.register({ firstName, lastName, email, password, confirmPassword });
      if (response.success) {
        setAccessToken(response.accessToken || null);
        setUser(response.user || null);
      } else {
        setError(response.message);
        throw new Error(response.message);
      }
    } catch (err: any) {
      setError(err.message || 'Registration failed');
      throw err;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const logout = useCallback(async () => {
    await authService.logout();
    setUser(null);
    setAccessToken(null);
    setError(null);
    // clear persisted tokens
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    clearAuthTokens();
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
    isAdmin: hasRole(user?.role, 'Admin'),
    isManager: hasRole(user?.role, 'Manager'),
    canManage: hasRole(user?.role, 'Admin', 'Manager'),
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
