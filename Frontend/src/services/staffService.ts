import apiClient from './apiClient';
import { Staff, CreateStaffDto, UpdateStaffDto, Pagination } from '../types';

interface StaffListResponse {
  success: boolean;
  data: Staff[];
  pagination: Pagination;
  message: string;
}

interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message: string;
}

export const staffService = {
  async getStaff(page = 1, pageSize = 10, search?: string, clubId?: number) {
    try {
      const params: any = { page, pageSize };
      if (search) params.search = search;
      if (clubId) params.clubId = clubId;

      const response = await apiClient.get<StaffListResponse>('/staff', { params });
      if (response.data.success) {
        return { staff: response.data.data, pagination: response.data.pagination };
      }
      throw new Error(response.data.message);
    } catch (error: any) {
      throw new Error(error.response?.data?.message || error.message || 'Failed to fetch staff');
    }
  },

  async createStaff(payload: CreateStaffDto): Promise<Staff> {
    try {
      const response = await apiClient.post<ApiResponse<Staff>>('/staff', payload);
      if (response.data.success && response.data.data) return response.data.data;
      throw new Error(response.data.message);
    } catch (error: any) {
      throw new Error(error.response?.data?.message || error.message || 'Failed to create staff');
    }
  },

  async updateStaff(id: number, payload: UpdateStaffDto): Promise<Staff> {
    try {
      const response = await apiClient.put<ApiResponse<Staff>>(`/staff/${id}`, payload);
      if (response.data.success && response.data.data) return response.data.data;
      throw new Error(response.data.message);
    } catch (error: any) {
      throw new Error(error.response?.data?.message || error.message || 'Failed to update staff');
    }
  },

  async deleteStaff(id: number): Promise<void> {
    try {
      const response = await apiClient.delete<ApiResponse<void>>(`/staff/${id}`);
      if (!response.data.success) throw new Error(response.data.message);
    } catch (error: any) {
      throw new Error(error.response?.data?.message || error.message || 'Failed to delete staff');
    }
  }
};
