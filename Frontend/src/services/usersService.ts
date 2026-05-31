import apiClient from './apiClient';
import { User } from '../types';

interface GetUsersResponse {
  success: boolean;
  data: User[];
  pagination: {
    currentPage: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
  };
  message: string;
}

export const usersService = {
  getUsers: async (page = 1, pageSize = 10, search = '') => {
    const params: Record<string, any> = { page, pageSize };
    if (search) params.search = search;
    const res = await apiClient.get<GetUsersResponse>('/users', { params });
    return res.data;
  },

  getUserById: async (id: string) => {
    const res = await apiClient.get<{ success: boolean; data: User }>(`/users/${id}`);
    return res.data;
  },

  createUser: async (payload: { email: string; password: string; firstName: string; lastName: string; role: string }) => {
    const body = {
      email: payload.email,
      password: payload.password,
      firstName: payload.firstName,
      lastName: payload.lastName,
      role: payload.role,
    };
    const res = await apiClient.post('/users', body);
    return res.data;
  },

  updateUser: async (id: string, payload: { email: string; firstName: string; lastName: string }) => {
    const res = await apiClient.put(`/users/${id}`, payload);
    return res.data;
  },

  deleteUser: async (id: string) => {
    const res = await apiClient.delete(`/users/${id}`);
    return res.data;
  },
};

export type { GetUsersResponse };
