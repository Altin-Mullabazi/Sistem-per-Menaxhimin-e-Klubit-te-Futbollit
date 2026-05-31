import axios, { AxiosInstance, AxiosError, InternalAxiosRequestConfig } from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5005/api';

let accessToken: string | null = null;
let refreshToken: string | null = null;

const apiClient: AxiosInstance = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to add access token
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor to handle 401 and refresh token
apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    if (error.response?.status === 401 && !originalRequest._retry && refreshToken) {
      originalRequest._retry = true;

      try {
        const response = await axios.post(`${API_URL}/auth/refresh`, {
          refreshToken,
        });

        if (response.data.success) {
          accessToken = response.data.accessToken;
          refreshToken = response.data.refreshToken;

          // Dispatch event to update tokens in context
          window.dispatchEvent(
            new CustomEvent('tokenRefreshed', {
              detail: { accessToken, refreshToken },
            })
          );

          // Retry original request with new token
          if (originalRequest.headers) {
            originalRequest.headers.Authorization = `Bearer ${accessToken}`;
          }
          return apiClient(originalRequest);
        }
      } catch (refreshError) {
        // Refresh failed, logout user
        window.dispatchEvent(new Event('tokenExpired'));
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export const setAuthTokens = (access: string, refresh: string) => {
  accessToken = access;
  refreshToken = refresh;
};

export const clearAuthTokens = () => {
  accessToken = null;
  refreshToken = null;
};

export default apiClient;
