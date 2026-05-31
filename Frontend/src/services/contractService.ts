import apiClient from './apiClient';
import { Contract, CreateContractDto, UpdateContractDto, PagedResult } from '../types';

export const contractService = {
  getContracts: async (page = 1, pageSize = 10): Promise<PagedResult<Contract>> => {
    try {
      const response = await apiClient.get('/contracts', { params: { page, pageSize } });
      return response.data.data || { items: [], pagination: { page, pageSize, totalCount: 0, totalPages: 0 } };
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to fetch contracts' };
    }
  },

  getActive: async (): Promise<Contract[]> => {
    try {
      const response = await apiClient.get('/contracts/active');
      return response.data.data || [];
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to fetch active contracts' };
    }
  },

  getExpiring: async (): Promise<Contract[]> => {
    try {
      const response = await apiClient.get('/contracts/expiring');
      return response.data.data || [];
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to fetch expiring contracts' };
    }
  },

  createContract: async (payload: CreateContractDto) => {
    try {
      const response = await apiClient.post('/contracts', payload);
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to create contract' };
    }
  },

  updateContract: async (id: number, payload: UpdateContractDto) => {
    try {
      const response = await apiClient.put(`/contracts/${id}`, payload);
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to update contract' };
    }
  },

  deleteContract: async (id: number) => {
    try {
      const response = await apiClient.delete(`/contracts/${id}`);
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to delete contract' };
    }
  },
};

export default contractService;
