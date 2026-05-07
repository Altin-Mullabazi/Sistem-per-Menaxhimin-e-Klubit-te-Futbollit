import apiClient, { setAuthTokens, clearAuthTokens } from './apiClient';
import { LoginRequest, RegisterRequest, AuthResponse, RegisterResponse } from '../types';

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
      const response = await apiClient.post<RegisterResponse>('/auth/register', payload);
      const registerResponse = response.data;
      const tokens = registerResponse.data?.tokens;

      if (registerResponse.success && tokens?.accessToken && tokens.refreshToken) {
        setAuthTokens(tokens.accessToken, tokens.refreshToken);
        return {
          success: true,
          message: registerResponse.message,
          accessToken: tokens.accessToken,
          refreshToken: tokens.refreshToken,
          user: {
            id: registerResponse.data!.userId,
            username: registerResponse.data!.email,
            email: registerResponse.data!.email,
          },
        };
      }

      return {
        success: false,
        message: registerResponse.errors?.[0]?.message || registerResponse.message,
      };
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
