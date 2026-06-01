import React, { useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { Match, Pagination, Club, Season, Stadium } from '../types';
import { matchService } from '../services/matchService';
import { clubService } from '../services/clubService';
import { seasonService } from '../services/seasonService';
import { stadiumService } from '../services/stadiumService';
import { Match, Pagination, Club } from '../types';
import { matchService } from '../services/matchService';
import MatchTable from '../components/MatchTable';
import MatchForm from '../components/MatchForm';
import MatchFilters from '../components/MatchFilters';
import '../styles/Matches.css';

const Matches: React.FC = () => {
// Mock data - in real app, fetch from backend
const MOCK_CLUBS: Club[] = [
  { id: 1, name: 'Manchester United' },
  { id: 2, name: 'Liverpool' },
  { id: 3, name: 'Manchester City' },
  { id: 4, name: 'Arsenal' },
];

const MOCK_SEASONS = [
  { id: 1, name: '2025-2026', startDate: '2025-08-01', endDate: '2026-05-31' },
  { id: 2, name: '2024-2025', startDate: '2024-08-01', endDate: '2025-05-31' },
];

const MOCK_STADIUMS = [
  { id: 1, name: 'Old Trafford', city: 'Manchester' },
  { id: 2, name: 'Anfield', city: 'Liverpool' },
  { id: 3, name: 'Etihad Stadium', city: 'Manchester' },
  { id: 4, name: 'Emirates Stadium', city: 'London' },
];

export const Matches: React.FC = () => {
  const { user } = useAuth();
  const [matches, setMatches] = useState<Match[]>([]);
  const [pagination, setPagination] = useState<Pagination>({
    page: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0,
  });

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const [showForm, setShowForm] = useState(false);
  const [editingMatch, setEditingMatch] = useState<Match | null>(null);

  const [clubs, setClubs] = useState<Club[]>([]);
  const [seasons, setSeasons] = useState<Season[]>([]);
  const [stadiums, setStadiums] = useState<Stadium[]>([]);

  // Filters
  const [clubId, setClubId] = useState<number | undefined>();
  const [seasonId, setSeasonId] = useState<number | undefined>();
  const [status, setStatus] = useState<string | undefined>();
  const [startDate, setStartDate] = useState<string>('');
  const [endDate, setEndDate] = useState<string>('');

  const canCreate = user?.role === 'Admin' || user?.role === 'Manager';
  const canEdit = user?.role === 'Admin' || user?.role === 'Manager';
  const canDelete = user?.role === 'Admin';

  const loadMatches = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await matchService.getMatches(
        pagination.page,
        pagination.pageSize,
        clubId,
        seasonId,
        status,
        startDate || undefined,
        endDate || undefined
        status
      );
      setMatches(data.matches);
      setPagination(data.pagination);
    } catch (err: any) {
      setError(err.message || 'Failed to load matches');
    } finally {
      setIsLoading(false);
    }
  };

  const loadFilterData = async () => {
    try {
      const [clubsData, seasonsData, stadiumsData] = await Promise.all([
        clubService.getClubs(1, 100),
        seasonService.getSeasons(1, 100),
        stadiumService.getStadiums(1, 100),
      ]);

      setClubs(clubsData);
      setSeasons(seasonsData.data);
      setStadiums(stadiumsData);
    } catch (err: any) {
      console.warn('Unable to load filter data', err);
    }
  };

  useEffect(() => {
    loadFilterData();
  }, []);

  useEffect(() => {
    loadMatches();
  }, [pagination.page, pagination.pageSize, clubId, seasonId, status, startDate, endDate]);
  useEffect(() => {
    loadMatches();
  }, [pagination.page, pagination.pageSize, clubId, seasonId, status]);

  const handleCreateClick = () => {
    if (!canCreate) {
      setError('You do not have permission to create matches');
      return;
    }
    setEditingMatch(null);
    setShowForm(true);
  };

  const handleEditClick = (match: Match) => {
    if (!canEdit) {
      setError('You do not have permission to edit matches');
      return;
    }
    setEditingMatch(match);
    setShowForm(true);
  };

  const handleFormClose = () => {
    setShowForm(false);
    setEditingMatch(null);
  };

  const handleFormSubmit = async (data: any) => {
    setError(null);
    try {
      if (editingMatch) {
        await matchService.updateMatch(editingMatch.id, data);
        setSuccessMessage('Match updated successfully');
      } else {
        await matchService.createMatch(data);
        setSuccessMessage('Match created successfully');
      }
      handleFormClose();
      await loadMatches();
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(err.message);
    }
  };

  const handleDeleteMatch = async (id: number) => {
    if (!canDelete) {
      setError('You do not have permission to delete matches');
      return;
    }

    if (window.confirm('Are you sure you want to delete this match? This action cannot be undone.')) {
      setError(null);
      try {
        await matchService.deleteMatch(id);
        setSuccessMessage('Match deleted successfully');
        setMatches(matches.filter((m) => m.id !== id));
        setTimeout(() => setSuccessMessage(null), 3000);
      } catch (err: any) {
        setError(err.message);
      }
    }
  };

  const handleClubChange = (newClubId?: number) => {
    setClubId(newClubId);
    setPagination((prev) => ({ ...prev, page: 1 }));
  };

  const handleSeasonChange = (newSeasonId?: number) => {
    setSeasonId(newSeasonId);
    setPagination((prev) => ({ ...prev, page: 1 }));
  };

  const handleStatusChange = (newStatus?: string) => {
    setStatus(newStatus);
    setPagination((prev) => ({ ...prev, page: 1 }));
  };

  const handleStartDateChange = (newDate?: string) => {
    setStartDate(newDate || '');
    setPagination((prev) => ({ ...prev, page: 1 }));
  };

  const handleEndDateChange = (newDate?: string) => {
    setEndDate(newDate || '');
    setPagination((prev) => ({ ...prev, page: 1 }));
  };

  const handlePageChange = (page: number) => {
    setPagination((prev) => ({ ...prev, page }));
  };

  const handlePageSizeChange = (size: number) => {
    setPagination((prev) => ({ ...prev, pageSize: size, page: 1 }));
  };

  return (
    <div className="matches-container">
      <div className="matches-header">
        <div>
          <h1>Matches Management</h1>
          <p className="header-subtitle">Manage football club matches, schedules, and results</p>
        </div>
        <div className="header-info">
          <p>Logged in as: <strong>{user?.username || 'User'}</strong></p>
          {user?.role && <p>Role: <strong>{user.role}</strong></p>}
        </div>
      </div>

      {error && (
        <div className="alert alert-error">
          {error}
          <button className="close-btn" onClick={() => setError(null)}>×</button>
        </div>
      )}

      {successMessage && (
        <div className="alert alert-success">
          {successMessage}
          <button className="close-btn" onClick={() => setSuccessMessage(null)}>×</button>
        </div>
      )}

      <div className="matches-controls">
        {canCreate && (
          <button
            className="btn btn-primary btn-lg"
            onClick={handleCreateClick}
            disabled={isLoading || showForm}
          >
            + Create New Match
          </button>
        )}
      </div>

      <MatchFilters
        clubId={clubId}
        seasonId={seasonId}
        status={status}
        startDate={startDate}
        endDate={endDate}
        page={pagination.page}
        pageSize={pagination.pageSize}
        totalPages={pagination.totalPages}
        clubs={clubs}
        seasons={seasons}
        onClubChange={handleClubChange}
        onSeasonChange={handleSeasonChange}
        onStatusChange={handleStatusChange}
        onStartDateChange={handleStartDateChange}
        onEndDateChange={handleEndDateChange}
        page={pagination.page}
        pageSize={pagination.pageSize}
        totalPages={pagination.totalPages}
        clubs={MOCK_CLUBS}
        seasons={MOCK_SEASONS}
        onClubChange={handleClubChange}
        onSeasonChange={handleSeasonChange}
        onStatusChange={handleStatusChange}
        onPageChange={handlePageChange}
        onPageSizeChange={handlePageSizeChange}
      />

      <MatchTable
        matches={matches}
        isLoading={isLoading}
        onEdit={handleEditClick}
        onDelete={handleDeleteMatch}
        userRole={user?.role}
      />

      {showForm && (
        <MatchForm
          match={editingMatch}
          clubs={clubs}
          seasons={seasons}
          stadiums={stadiums}
          clubs={MOCK_CLUBS}
          seasons={MOCK_SEASONS}
          stadiums={MOCK_STADIUMS}
          isLoading={isLoading}
          onSubmit={handleFormSubmit}
          onClose={handleFormClose}
        />
      )}
    </div>
  );
};

export default Matches;
