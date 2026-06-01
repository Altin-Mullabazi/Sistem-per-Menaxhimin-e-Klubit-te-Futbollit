import apiClient from './apiClient';
import { TrainingSession, CreateTrainingSessionDto, UpdateTrainingSessionDto, Pagination } from '../types';

interface TrainingSessionListResponse {
  success: boolean;
  data: TrainingSession[];
  pagination: Pagination;
  message: string;
}

interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message: string;
}

export const trainingService = {
  async getTrainingSessions(
    page = 1,
    pageSize = 10,
    clubId?: number,
    startDate?: string,
    endDate?: string
  ): Promise<{ sessions: TrainingSession[]; pagination: Pagination }> {
    try {
      const params: any = { page, pageSize };
      if (clubId) params.clubId = clubId;
      if (startDate) params.startDate = startDate;
      if (endDate) params.endDate = endDate;

      const response = await apiClient.get<TrainingSessionListResponse>('/trainingSessions', { params });
      if (response.data.success) {
        return {
          sessions: response.data.data,
          pagination: response.data.pagination,
        };
      }

      throw new Error(response.data.message);
    } catch (error: any) {
      throw new Error(error.response?.data?.message || error.message || 'Failed to fetch training sessions');
    }
  },

  async createTrainingSession(payload: CreateTrainingSessionDto): Promise<TrainingSession> {
    try {
      const response = await apiClient.post<ApiResponse<TrainingSession>>('/trainingSessions', payload);
      if (response.data.success && response.data.data) {
        return response.data.data;
      }
      throw new Error(response.data.message);
    } catch (error: any) {
      throw new Error(error.response?.data?.message || error.message || 'Failed to create training session');
    }
  },

  async updateTrainingSession(id: number, payload: UpdateTrainingSessionDto): Promise<TrainingSession> {
    try {
      const response = await apiClient.put<ApiResponse<TrainingSession>>(`/trainingSessions/${id}`, payload);
      if (response.data.success && response.data.data) {
        return response.data.data;
      }
      throw new Error(response.data.message);
    } catch (error: any) {
      throw new Error(error.response?.data?.message || error.message || 'Failed to update training session');
    }
  },

  async deleteTrainingSession(id: number): Promise<void> {
    try {
      const response = await apiClient.delete<ApiResponse<void>>(`/trainingSessions/${id}`);
      if (!response.data.success) {
        throw new Error(response.data.message);
      }
    } catch (error: any) {
      throw new Error(error.response?.data?.message || error.message || 'Failed to delete training session');
    }
  },
};
