import apiClient, { setAuthTokens, clearAuthTokens } from './apiClient';
import { LoginRequest, AuthResponse } from '../types';

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

  logout: () => {
    clearAuthTokens();
  },
};
