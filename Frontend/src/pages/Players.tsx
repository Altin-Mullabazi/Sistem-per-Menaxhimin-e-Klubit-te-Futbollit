import React, { useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { Player } from '../types';
import { playerService } from '../services/playerService';
import PlayerForm from '../components/PlayerForm';
import PlayerList from '../components/PlayerList';
import '../styles/Players.css';

export const Players: React.FC = () => {
  const { user } = useAuth();
  const [players, setPlayers] = useState<Player[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editingPlayer, setEditingPlayer] = useState<Player | null>(null);

  const loadPlayers = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const fetchedPlayers = await playerService.getAllPlayers();
      setPlayers(fetchedPlayers);
    } catch (err: any) {
      setError(err.message || 'Failed to load players');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadPlayers();
  }, []);

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
    await loadPlayers();
    handleFormClose();
  };

  const handleDeletePlayer = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this player?')) {
      try {
        await playerService.deletePlayer(id);
        setPlayers(players.filter((p) => p.id !== id));
      } catch (err: any) {
        setError(err.message || 'Failed to delete player');
      }
    }
  };

  return (
    <div className="players-container">
      <div className="players-header">
        <h1>Football Players Management</h1>
        <div className="header-info">
          <p>Logged in as: <strong>{user?.username}</strong></p>
        </div>
      </div>

      {error && <div className="error-message">{error}</div>}

      <button
        className="btn btn-primary"
        onClick={handleCreateClick}
        disabled={isLoading || showForm}
      >
        + Add New Player
      </button>

      {showForm && (
        <PlayerForm
          player={editingPlayer}
          onClose={handleFormClose}
          onSubmit={handleFormSubmit}
        />
      )}

      {isLoading && <div className="loading">Loading players...</div>}

      {!isLoading && players.length === 0 && (
        <div className="empty-state">
          <p>No players found. Start by adding a new player!</p>
        </div>
      )}

      {!isLoading && players.length > 0 && (
        <PlayerList
          players={players}
          onEdit={handleEditClick}
          onDelete={handleDeletePlayer}
        />
      )}
    </div>
  );
};

export default Players;
