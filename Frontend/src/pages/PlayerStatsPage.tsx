import React, { useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { playerService } from '../services/playerService';
import { matchService } from '../services/matchService';
import { playerStatsService } from '../services/playerStatsService';
import { Player, Match, PlayerStats, Pagination, TopScorer, CreatePlayerStatsDto, UpdatePlayerStatsDto } from '../types';
import StatsTable from '../components/StatsTable';
import RankingsSection from '../components/RankingsSection';
import '../styles/PlayerStats.css';

const initialFormState = {
  playerId: 0,
  matchId: 0,
  goalsScored: 0,
  assists: 0,
  yellowCards: 0,
  redCards: 0,
  minutesPlayed: 0,
};

export const PlayerStatsPage: React.FC = () => {
  const { user } = useAuth();
  const [players, setPlayers] = useState<Player[]>([]);
  const [matches, setMatches] = useState<Match[]>([]);
  const [stats, setStats] = useState<PlayerStats[]>([]);
  const [pagination, setPagination] = useState<Pagination>({ page: 1, pageSize: 10, totalCount: 0, totalPages: 0 });
  const [playerFilter, setPlayerFilter] = useState<number | undefined>();
  const [matchFilter, setMatchFilter] = useState<number | undefined>();
  const [sortBy, setSortBy] = useState<string>('goals');
  const [topScorers, setTopScorers] = useState<TopScorer[]>([]);
  const [topAssists, setTopAssists] = useState<TopScorer[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isProcessing, setIsProcessing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editingStat, setEditingStat] = useState<PlayerStats | null>(null);
  const [formData, setFormData] = useState<CreatePlayerStatsDto & { playerId: number; matchId: number }>({
    ...initialFormState,
  });

  const canManage = user?.role === 'Admin' || user?.role === 'Manager';
  const canDelete = user?.role === 'Admin';

  const loadLookups = async () => {
    try {
      const [playersData, matchesData] = await Promise.all([
        playerService.getAllPlayers(),
        matchService.getMatches(1, 100),
      ]);
      setPlayers(playersData);
      setMatches(matchesData.matches);
    } catch (err: any) {
      setError(err.message || 'Failed to load players or matches');
    }
  };

  const loadStats = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await playerStatsService.getPlayerStats(
        pagination.page,
        pagination.pageSize,
        playerFilter,
        matchFilter,
        sortBy
      );
      setStats(data.stats);
      setPagination(data.pagination);
    } catch (err: any) {
      setError(err.message || 'Failed to load player stats');
    } finally {
      setIsLoading(false);
    }
  };

  const loadRankings = async () => {
    try {
      const [scorers, assists] = await Promise.all([
        playerStatsService.getTopScorers(10),
        playerStatsService.getTopAssists(10),
      ]);
      setTopScorers(scorers);
      setTopAssists(assists);
    } catch (err: any) {
      setError(err.message || 'Failed to load rankings');
    }
  };

  useEffect(() => {
    loadLookups();
    loadRankings();
  }, []);

  useEffect(() => {
    loadStats();
  }, [pagination.page, pagination.pageSize, playerFilter, matchFilter, sortBy]);

  const handlePageChange = (page: number) => {
    setPagination((prev) => ({ ...prev, page }));
  };

  const handlePageSizeChange = (size: number) => {
    setPagination((prev) => ({ ...prev, pageSize: size, page: 1 }));
  };

  const handleFilterChange = (field: 'player' | 'match', value?: number) => {
    if (field === 'player') {
      setPlayerFilter(value);
    } else {
      setMatchFilter(value);
    }
    setPagination((prev) => ({ ...prev, page: 1 }));
  };

  const handleCreateClick = () => {
    setEditingStat(null);
    setFormData({ ...initialFormState });
    setShowForm(true);
  };

  const handleEditClick = (stat: PlayerStats) => {
    setEditingStat(stat);
    setFormData({
      playerId: stat.playerId,
      matchId: stat.matchId,
      goalsScored: stat.goalsScored,
      assists: stat.assists,
      yellowCards: stat.yellowCards,
      redCards: stat.redCards,
      minutesPlayed: stat.minutesPlayed,
    });
    setShowForm(true);
  };

  const handleDeleteClick = async (id: number) => {
    if (!canDelete) {
      setError('You do not have permission to delete stats.');
      return;
    }

    if (!window.confirm('Delete this player stats entry? This action cannot be undone.')) {
      return;
    }

    setIsProcessing(true);
    setError(null);
    try {
      await playerStatsService.deletePlayerStats(id);
      setSuccessMessage('Player stats deleted successfully');
      loadStats();
      loadRankings();
    } catch (err: any) {
      setError(err.message || 'Failed to delete player stats');
    } finally {
      setIsProcessing(false);
      setTimeout(() => setSuccessMessage(null), 3000);
    }
  };

  const handleFormChange = (event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = event.target;
    setFormData((prev) => ({
      ...prev,
      [name]: Number(value),
    }));
  };

  const validateForm = () => {
    if (!formData.playerId || !formData.matchId) {
      setError('Please select a player and match.');
      return false;
    }
    if (formData.goalsScored < 0 || formData.assists < 0 || formData.yellowCards < 0 || formData.redCards < 0) {
      setError('Scores and cards cannot be negative.');
      return false;
    }
    if (formData.minutesPlayed < 0 || formData.minutesPlayed > 120) {
      setError('Minutes must be between 0 and 120.');
      return false;
    }
    return true;
  };

  const handleFormSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setError(null);

    if (!validateForm()) {
      return;
    }

    setIsProcessing(true);
    try {
      if (editingStat) {
        const updateData: UpdatePlayerStatsDto = {
          goalsScored: formData.goalsScored,
          assists: formData.assists,
          yellowCards: formData.yellowCards,
          redCards: formData.redCards,
          minutesPlayed: formData.minutesPlayed,
        };
        await playerStatsService.updatePlayerStats(editingStat.id, updateData);
        setSuccessMessage('Player stats updated successfully');
      } else {
        const createData: CreatePlayerStatsDto = {
          playerId: formData.playerId,
          matchId: formData.matchId,
          goalsScored: formData.goalsScored,
          assists: formData.assists,
          yellowCards: formData.yellowCards,
          redCards: formData.redCards,
          minutesPlayed: formData.minutesPlayed,
        };
        await playerStatsService.createPlayerStats(createData);
        setSuccessMessage('Player stats created successfully');
      }
      setShowForm(false);
      setEditingStat(null);
      loadStats();
      loadRankings();
    } catch (err: any) {
      setError(err.message || 'Failed to save stats');
    } finally {
      setIsProcessing(false);
      setTimeout(() => setSuccessMessage(null), 3000);
    }
  };

  return (
    <div className="player-stats-page">
      <header className="page-header">
        <div>
          <h1>Player Stats & Rankings</h1>
          <p className="page-description">Track player performance, sort by top scorers and assists, and manage stats with full authorization.</p>
        </div>
        <div className="header-actions">
          {canManage && (
            <button className="btn btn-primary" onClick={handleCreateClick} disabled={isProcessing || isLoading}>
              + Add Stats
            </button>
          )}
        </div>
      </header>

      {(error || successMessage) && (
        <div className={`alert ${error ? 'alert-error' : 'alert-success'}`}>
          {error || successMessage}
          <button className="close-btn" onClick={() => { setError(null); setSuccessMessage(null); }}>×</button>
        </div>
      )}

      <div className="stats-controls-grid">
        <div className="filter-group">
          <label htmlFor="playerFilter">Player</label>
          <select
            id="playerFilter"
            value={playerFilter ?? ''}
            onChange={(e) => handleFilterChange('player', e.target.value ? Number(e.target.value) : undefined)}
          >
            <option value="">All Players</option>
            {players.map((player) => (
              <option key={player.id} value={player.id}>
                {player.firstName} {player.lastName}
              </option>
            ))}
          </select>
        </div>

        <div className="filter-group">
          <label htmlFor="matchFilter">Match</label>
          <select
            id="matchFilter"
            value={matchFilter ?? ''}
            onChange={(e) => handleFilterChange('match', e.target.value ? Number(e.target.value) : undefined)}
          >
            <option value="">All Matches</option>
            {matches.map((match) => (
              <option key={match.id} value={match.id}>
                {match.homeClubName} vs {match.awayClubName} • {new Date(match.matchDate).toLocaleDateString()}
              </option>
            ))}
          </select>
        </div>

        <div className="filter-group">
          <label htmlFor="sortBy">Sort by</label>
          <select id="sortBy" value={sortBy} onChange={(e) => setSortBy(e.target.value)}>
            <option value="goals">Goals</option>
            <option value="assists">Assists</option>
          </select>
        </div>
      </div>

      <StatsTable
        stats={stats}
        isLoading={isLoading}
        onEdit={handleEditClick}
        onDelete={handleDeleteClick}
        canEdit={canManage}
        canDelete={canDelete}
      />

      <div className="pagination-controls">
        <div className="pagination-actions">
          <button className="btn btn-secondary" onClick={() => handlePageChange(Math.max(1, pagination.page - 1))} disabled={pagination.page === 1 || isLoading}>
            Previous
          </button>
          <span>
            Page {pagination.page} of {pagination.totalPages || 1}
          </span>
          <button className="btn btn-secondary" onClick={() => handlePageChange(Math.min(pagination.totalPages || 1, pagination.page + 1))} disabled={pagination.page === pagination.totalPages || isLoading || pagination.totalPages === 0}>
            Next
          </button>
        </div>
        <div className="page-size-control">
          <label htmlFor="pageSize">Items per page</label>
          <select id="pageSize" value={pagination.pageSize} onChange={(e) => handlePageSizeChange(Number(e.target.value))}>
            <option value={5}>5</option>
            <option value={10}>10</option>
            <option value={20}>20</option>
            <option value={50}>50</option>
          </select>
        </div>
      </div>

      <div className="rankings-grid">
        <RankingsSection title="Top Scorers" items={topScorers} metricName="Goals" primaryValueKey="goalsScored" />
        <RankingsSection title="Top Assists" items={topAssists} metricName="Assists" primaryValueKey="assists" />
      </div>

      {showForm && (
        <div className="modal-overlay" onClick={() => setShowForm(false)}>
          <div className="modal-card" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>{editingStat ? 'Edit Player Stats' : 'Create Player Stats'}</h2>
              <button className="close-btn" onClick={() => setShowForm(false)}>×</button>
            </div>
            <form className="stats-form" onSubmit={handleFormSubmit}>
              <div className="form-grid">
                <div className="form-group">
                  <label htmlFor="playerId">Player</label>
                  <select
                    id="playerId"
                    name="playerId"
                    value={formData.playerId}
                    onChange={handleFormChange}
                    disabled={Boolean(editingStat)}
                    required
                  >
                    <option value={0}>Select player</option>
                    {players.map((player) => (
                      <option key={player.id} value={player.id}>
                        {player.firstName} {player.lastName}
                      </option>
                    ))}
                  </select>
                </div>

                <div className="form-group">
                  <label htmlFor="matchId">Match</label>
                  <select
                    id="matchId"
                    name="matchId"
                    value={formData.matchId}
                    onChange={handleFormChange}
                    disabled={Boolean(editingStat)}
                    required
                  >
                    <option value={0}>Select match</option>
                    {matches.map((match) => (
                      <option key={match.id} value={match.id}>
                        {match.homeClubName} vs {match.awayClubName} • {new Date(match.matchDate).toLocaleDateString()}
                      </option>
                    ))}
                  </select>
                </div>

                <div className="form-group">
                  <label htmlFor="goalsScored">Goals Scored</label>
                  <input type="number" id="goalsScored" name="goalsScored" min={0} value={formData.goalsScored} onChange={handleFormChange} required />
                </div>

                <div className="form-group">
                  <label htmlFor="assists">Assists</label>
                  <input type="number" id="assists" name="assists" min={0} value={formData.assists} onChange={handleFormChange} required />
                </div>

                <div className="form-group">
                  <label htmlFor="yellowCards">Yellow Cards</label>
                  <input type="number" id="yellowCards" name="yellowCards" min={0} value={formData.yellowCards} onChange={handleFormChange} required />
                </div>

                <div className="form-group">
                  <label htmlFor="redCards">Red Cards</label>
                  <input type="number" id="redCards" name="redCards" min={0} value={formData.redCards} onChange={handleFormChange} required />
                </div>

                <div className="form-group">
                  <label htmlFor="minutesPlayed">Minutes Played</label>
                  <input type="number" id="minutesPlayed" name="minutesPlayed" min={0} max={120} value={formData.minutesPlayed} onChange={handleFormChange} required />
                </div>
              </div>

              <div className="modal-actions">
                <button type="button" className="btn btn-secondary" onClick={() => setShowForm(false)} disabled={isProcessing}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary" disabled={isProcessing}>
                  {isProcessing ? 'Saving...' : editingStat ? 'Update Stats' : 'Create Stats'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default PlayerStatsPage;
