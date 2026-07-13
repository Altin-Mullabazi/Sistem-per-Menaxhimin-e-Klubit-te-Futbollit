import axios, { AxiosInstance, AxiosError, InternalAxiosRequestConfig } from 'axios';

export const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

// Do not cache tokens at module init — read from storage at request time
let refreshPromise: Promise<any> | null = null;

const apiClient: AxiosInstance = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to add access token
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    try {
      const token = localStorage.getItem('accessToken');
      if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`;
      }
    } catch {
      // ignore storage errors
    }
    return config;
  },
  (error) => Promise.reject(error)
);

const isAuthEndpoint = (url?: string) =>
  !!url && (url.includes('/auth/login') || url.includes('/auth/register') || url.includes('/auth/refresh'));

// Response interceptor to handle 401 and refresh token
apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    if (isAuthEndpoint(originalRequest?.url)) {
      return Promise.reject(error);
    }

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      const storedRefresh = localStorage.getItem('refreshToken');
      if (!storedRefresh) {
        window.dispatchEvent(new Event('tokenExpired'));
        return Promise.reject(error);
      }

      try {
        if (!refreshPromise) {
          refreshPromise = axios
            .post(`${API_URL}/auth/refresh`, { refreshToken: storedRefresh })
            .then((res) => {
              refreshPromise = null;
              if (res.data && res.data.success) {
                // persist tokens
                try {
                  localStorage.setItem('accessToken', res.data.accessToken || '');
                  localStorage.setItem('refreshToken', res.data.refreshToken || '');
                } catch {}

                window.dispatchEvent(
                  new CustomEvent('tokenRefreshed', { detail: { accessToken: res.data.accessToken, refreshToken: res.data.refreshToken } })
                );
                return res.data;
              }
              throw new Error('Refresh failed');
            })
            .catch((rerr) => {
              refreshPromise = null;
              window.dispatchEvent(new Event('tokenExpired'));
              throw rerr;
            });
        }

        const tokens = await refreshPromise;

        // Retry original request with new token
        const newAccess = tokens.accessToken || localStorage.getItem('accessToken');
        if (originalRequest.headers && newAccess) {
          originalRequest.headers.Authorization = `Bearer ${newAccess}`;
        }

        return apiClient(originalRequest);
      } catch (refreshError) {
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export const setAuthTokens = (access: string, refresh: string) => {
  try {
    localStorage.setItem('accessToken', access || '');
    localStorage.setItem('refreshToken', refresh || '');
  } catch {}
  if (access) {
    apiClient.defaults.headers.common.Authorization = `Bearer ${access}`;
  }
};

export const clearAuthTokens = () => {
  try {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  } catch {}
  delete apiClient.defaults.headers.common.Authorization;
};

export default apiClient;
