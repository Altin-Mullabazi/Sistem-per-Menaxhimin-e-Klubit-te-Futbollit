import apiClient, { setAuthTokens, clearAuthTokens } from './apiClient';
import { LoginRequest, RegisterRequest, AuthResponse } from '../types';

export const authService = {
  login: async (credentials: LoginRequest): Promise<AuthResponse> => {
    try {
      const response = await apiClient.post('/auth/login', credentials);
      if (response.data.success) {
        setAuthTokens(response.data.accessToken, response.data.refreshToken);
      }
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Login failed' };
    }
  },

  register: async (payload: RegisterRequest): Promise<AuthResponse> => {
    try {
      const response = await apiClient.post('/auth/register', payload);
      if (response.data.success && response.data.accessToken && response.data.refreshToken) {
        setAuthTokens(response.data.accessToken, response.data.refreshToken);
      }
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Registration failed' };
    }
  },

  refreshToken: async (refreshTokenValue: string): Promise<AuthResponse> => {
    try {
      const response = await apiClient.post('/auth/refresh', {
        refreshToken: refreshTokenValue,
      });
      if (response.data.success) {
        setAuthTokens(response.data.accessToken, response.data.refreshToken);
      }
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Token refresh failed' };
    }
  },

  logout: async (): Promise<void> => {
    try {
      await apiClient.post('/auth/logout');
    } catch {
      // Always clear local auth state, even if backend logout fails.
    } finally {
      clearAuthTokens();
    }
  },
};
