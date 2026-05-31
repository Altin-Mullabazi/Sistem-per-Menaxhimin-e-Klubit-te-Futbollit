import apiClient from './apiClient';
import {
  DashboardSummary,
  ExpiringContract,
  InjuredPlayer,
  RecentTransfer,
  TopScorer,
  UpcomingMatch,
} from '../types';

export const dashboardService = {
  getSummary: async (): Promise<DashboardSummary> => {
    try {
      const response = await apiClient.get('/dashboard/summary');
      return response.data.data;
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to fetch dashboard summary' };
    }
  },

  getUpcomingMatches: async (): Promise<UpcomingMatch[]> => {
    try {
      const response = await apiClient.get('/dashboard/upcoming-matches');
      return response.data.data || [];
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to fetch upcoming matches' };
    }
  },

  getTopScorers: async (): Promise<TopScorer[]> => {
    try {
      const response = await apiClient.get('/dashboard/top-scorers');
      return response.data.data || [];
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to fetch top scorers' };
    }
  },

  getInjuredPlayers: async (): Promise<InjuredPlayer[]> => {
    try {
      const response = await apiClient.get('/dashboard/injured-players');
      return response.data.data || [];
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to fetch injured players' };
    }
  },

  getExpiringContracts: async (): Promise<ExpiringContract[]> => {
    try {
      const response = await apiClient.get('/dashboard/expiring-contracts');
      return response.data.data || [];
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to fetch expiring contracts' };
    }
  },

  getRecentTransfers: async (): Promise<RecentTransfer[]> => {
    try {
      const response = await apiClient.get('/dashboard/recent-transfers');
      return response.data.data || [];
    } catch (error: any) {
      throw error.response?.data || { success: false, message: 'Failed to fetch recent transfers' };
    }
  },
};
