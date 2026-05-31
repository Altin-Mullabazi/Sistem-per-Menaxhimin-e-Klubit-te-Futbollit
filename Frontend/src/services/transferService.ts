import apiClient from './apiClient';
import { Transfer, CreateTransferDto, UpdateTransferDto, PagedResult } from '../types';

interface TransferQuery {
  page?: number;
  pageSize?: number;
  player?: string;
  fromDate?: string;
  toDate?: string;
}

export const transferService = {
  getTransfers: async (query: TransferQuery = {}): Promise<PagedResult<Transfer>> => {
    try {
      const response = await apiClient.get('/transfers', { params: query });
      // Backend returns { success, data: transfersArray, pagination: { ... } }
      const data = response.data || {};
      const items: Transfer[] = data.data || [];
      const pagination = data.pagination || { page: query.page || 1, pageSize: query.pageSize || 10, totalCount: 0, totalPages: 0 };
      return { items, pagination };
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to fetch transfers' };
    }
  },

  createTransfer: async (payload: CreateTransferDto) => {
    try {
      const response = await apiClient.post('/transfers', payload);
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to create transfer' };
    }
  },

  updateTransfer: async (id: number, payload: UpdateTransferDto) => {
    try {
      const response = await apiClient.put(`/transfers/${id}`, payload);
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to update transfer' };
    }
  },

  deleteTransfer: async (id: number) => {
    try {
      const response = await apiClient.delete(`/transfers/${id}`);
      return response.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to delete transfer' };
    }
  },
};

export default transferService;
