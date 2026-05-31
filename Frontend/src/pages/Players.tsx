import React, { useEffect, useState, useCallback } from 'react';
import { useAuth } from '../context/AuthContext';
import { Player, PlayerListResponse, Club } from '../types';
import { playerService } from '../services/playerService';
import { clubService } from '../services/clubService';
import PlayerForm from '../components/PlayerForm';
import PlayerList from '../components/PlayerList';
import '../styles/Management.css';

export const Players: React.FC = () => {
  const { user } = useAuth();
  const [players, setPlayers] = useState<Player[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editingPlayer, setEditingPlayer] = useState<Player | null>(null);

  // Pagination state
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  // Filter and search state
  const [searchTerm, setSearchTerm] = useState('');
  const [positionFilter, setPositionFilter] = useState('');
  const [clubFilter, setClubFilter] = useState('');
  const [clubs, setClubs] = useState<Club[]>([]);
  const [positions, setPositions] = useState<string[]>([]);

  // Debounce timer
  const [searchTimeout, setSearchTimeout] = useState<ReturnType<typeof setTimeout> | null>(null);

  const positions_list = ['Goalkeeper', 'Defender', 'Midfielder', 'Forward', 'Left Winger', 'Right Winger'];

  const loadPlayers = useCallback(async (page: number = 1, search?: string, position?: string, clubId?: number) => {
    setIsLoading(true);
    setError(null);
    try {
      const response: PlayerListResponse = await playerService.getPlayers(page, pageSize, search, position, clubId);
      console.log('loadPlayers response:', response);
      setPlayers(response?.data || []);
      setTotalPages(response?.totalPages || 1);
      setTotalCount(response?.totalCount || 0);
      setCurrentPage(page);
    } catch (err: any) {
      console.error('Failed to load players:', err);
      setError(err.message || 'Failed to load players');
    } finally {
      setIsLoading(false);
    }
  }, [pageSize]);

  useEffect(() => {
    loadPlayers(1);
  }, [loadPlayers]);

  // Load clubs for filter
  useEffect(() => {
    const loadClubs = async () => {
      try {
        const allClubs = await clubService.getAllClubs();
        setClubs(allClubs);
      } catch (err: any) {
        console.error('Failed to load clubs:', err);
      }
    };
    loadClubs();
  }, []);

  // Load positions
  useEffect(() => {
    setPositions(positions_list);
  }, []);

  // Debounced search
  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setSearchTerm(value);

    if (searchTimeout) clearTimeout(searchTimeout);
    const timeout = setTimeout(() => {
      const clubId = clubFilter ? parseInt(clubFilter) : undefined;
      loadPlayers(1, value, positionFilter, clubId);
    }, 500);
    setSearchTimeout(timeout);
  };

  const handlePositionFilter = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const value = e.target.value;
    setPositionFilter(value);
    const clubId = clubFilter ? parseInt(clubFilter) : undefined;
    loadPlayers(1, searchTerm, value, clubId);
  };

  const handleClubFilter = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const value = e.target.value;
    setClubFilter(value);
    const clubId = value ? parseInt(value) : undefined;
    loadPlayers(1, searchTerm, positionFilter, clubId);
  };

  const handleCreateClick = () => {
    setEditingPlayer(null);
    setShowForm(true);
  };

  const handleEditClick = (player: Player) => {
    setEditingPlayer(player);
    setShowForm(true);
  };

  const handleFormClose = () => {
    setShowForm(false);
    setEditingPlayer(null);
  };

  const handleFormSubmit = async () => {
    const action = editingPlayer ? 'updated' : 'created';
    setSuccess(`Player ${action} successfully!`);
    setTimeout(() => setSuccess(null), 3000);
    const clubId = clubFilter ? parseInt(clubFilter) : undefined;
    await loadPlayers(1, searchTerm, positionFilter, clubId);
    handleFormClose();
  };

  const handleDeletePlayer = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this player?')) {
      try {
        await playerService.deletePlayer(id);
        setSuccess('Player deleted successfully!');
        setTimeout(() => setSuccess(null), 3000);
        const clubId = clubFilter ? parseInt(clubFilter) : undefined;
        await loadPlayers(currentPage, searchTerm, positionFilter, clubId);
      } catch (err: any) {
        setError(err.message || 'Failed to delete player');
      }
    }
  };

  const handlePageChange = (newPage: number) => {
    const clubId = clubFilter ? parseInt(clubFilter) : undefined;
    loadPlayers(newPage, searchTerm, positionFilter, clubId);
  };

  // Role-based permissions
  const canCreate = user?.role === 'Admin' || user?.role === 'Manager';
  const canEdit = user?.role === 'Admin' || user?.role === 'Manager';
  const canDelete = user?.role === 'Admin';

  return (
    <div className="management-container">
      <div className="management-header">
        <h1>👥 Players Management</h1>
        <div className="header-info">
          <p>Logged in as: <strong>{user?.username}</strong> ({user?.role})</p>
        </div>
      </div>

      {error && (
        <div className="alert alert-error">
          {error}
          <button className="alert-close" onClick={() => setError(null)}>✕</button>
        </div>
      )}

      {success && (
        <div className="alert alert-success">
          {success}
          <button className="alert-close" onClick={() => setSuccess(null)}>✕</button>
        </div>
      )}

      <div className="management-toolbar">
        <div className="toolbar-left">
          {canCreate && (
            <button
              className="btn btn-primary"
              onClick={handleCreateClick}
              disabled={isLoading || showForm}
            >
              + Add New Player
            </button>
          )}
        </div>

        <div className="toolbar-right">
          <div className="search-box">
            <input
              type="text"
              placeholder="Search by player name..."
              value={searchTerm}
              onChange={handleSearchChange}
              disabled={isLoading}
              className="search-input"
            />
            {searchTerm && (
              <button
                className="search-clear"
                onClick={() => {
                  setSearchTerm('');
                  const clubId = clubFilter ? parseInt(clubFilter) : undefined;
                  loadPlayers(1, '', positionFilter, clubId);
                }}
              >
                ✕
              </button>
            )}
          </div>

          <select
            className="filter-select"
            value={positionFilter}
            onChange={handlePositionFilter}
            disabled={isLoading}
          >
            <option value="">All Positions</option>
            {positions.map((pos) => (
              <option key={pos} value={pos}>
                {pos}
              </option>
            ))}
          </select>

          <select
            className="filter-select"
            value={clubFilter}
            onChange={handleClubFilter}
            disabled={isLoading}
          >
            <option value="">All Clubs</option>
            {clubs.map((club) => (
              <option key={club.id} value={club.id}>
                {club.name}
              </option>
            ))}
          </select>
        </div>
      </div>

      {showForm && (
        <PlayerForm
          player={editingPlayer}
          onClose={handleFormClose}
          onSubmit={handleFormSubmit}
          clubs={clubs}
        />
      )}

      <PlayerList
        players={players}
        isLoading={isLoading}
        onEdit={handleEditClick}
        onDelete={handleDeletePlayer}
        canEdit={canEdit}
        canDelete={canDelete}
      />

      {/* Pagination */}
      {!isLoading && totalPages > 1 && (
        <div className="pagination">
          <button
            className="pagination-btn"
            onClick={() => handlePageChange(currentPage - 1)}
            disabled={currentPage === 1}
          >
            ← Previous
          </button>

          <div className="pagination-info">
            Page {currentPage} of {totalPages} ({totalCount} total players)
          </div>

          <button
            className="pagination-btn"
            onClick={() => handlePageChange(currentPage + 1)}
            disabled={currentPage === totalPages}
          >
            Next →
          </button>
        </div>
      )}
    </div>
  );
};

export default Players;
