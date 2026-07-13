import React from 'react';
import { Player } from '../types';
import '../styles/List.css';

interface PlayerListProps {
  players: Player[];
  isLoading: boolean;
  onEdit: (player: Player) => void;
  onDelete: (id: number) => void;
  canEdit: boolean;
  canDelete: boolean;
}

const PlayerList: React.FC<PlayerListProps> = ({ players, isLoading, onEdit, onDelete, canEdit, canDelete }) => {
  if (isLoading) {
    return (
      <div className="loading">
        <div className="spinner"></div>
        <p>Loading players...</p>
      </div>
    );
  }

  if (!players || players.length === 0) {
    return (
      <div className="empty-state">
        <div className="empty-icon">👥</div>
        <p>No players found. Try adjusting your search or filters.</p>
      </div>
    );
  }

  return (
    <>
      {/* Desktop Table View */}
      <div className="list-desktop">
        <table className="players-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Position</th>
              <th>Age</th>
              <th>Jersey #</th>
              <th>Club</th>
              <th>Joined</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {(players || []).map((player) => (
              <tr key={player.id} className="player-row">
                <td className="player-name">
                  <strong>{player.firstName} {player.lastName}</strong>
                </td>
                <td>
                  <span className="position-badge">{player.position}</span>
                </td>
                <td className="center">{player.age}</td>
                <td className="center">{player.jerseyNumber || '—'}</td>
                <td>{player.clubName || '—'}</td>
                <td>{new Date(player.createdAt).toLocaleDateString()}</td>
                <td className="actions">
                  {canEdit && (
                    <button
                      className="btn btn-sm btn-secondary"
                      onClick={() => onEdit(player)}
                      title="Edit player"
                    >
                      Edit
                    </button>
                  )}
                  {canDelete && (
                    <button
                      className="btn btn-sm btn-danger"
                      onClick={() => onDelete(player.id)}
                      title="Delete player"
                    >
                      Delete
                    </button>
                  )}
                  {!canEdit && !canDelete && (
                    <span className="view-only">View Only</span>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Mobile Card View */}
      <div className="list-mobile">
        <div className="cards-grid">
          {(players || []).map((player) => (
            <div key={player.id} className="player-card">
              <div className="card-content">
                <h3 className="card-title">{player.firstName} {player.lastName}</h3>
                <div className="card-detail">
                  <span className="label">Position:</span>
                  <span className="position-badge">{player.position}</span>
                </div>
                <div className="card-detail">
                  <span className="label">Age:</span>
                  <span className="value">{player.age}</span>
                </div>
                <div className="card-detail">
                  <span className="label">Jersey:</span>
                  <span className="value">{player.jerseyNumber || '—'}</span>
                </div>
                <div className="card-detail">
                  <span className="label">Club:</span>
                  <span className="value">{player.clubName || '—'}</span>
                </div>
                <div className="card-detail">
                  <span className="label">Joined:</span>
                  <span className="value">{new Date(player.createdAt).toLocaleDateString()}</span>
                </div>
                <div className="card-actions">
                  {canEdit && (
                    <button
                      className="btn btn-sm btn-secondary"
                      onClick={() => onEdit(player)}
                    >
                      Edit
                    </button>
                  )}
                  {canDelete && (
                    <button
                      className="btn btn-sm btn-danger"
                      onClick={() => onDelete(player.id)}
                    >
                      Delete
                    </button>
                  )}
                  {!canEdit && !canDelete && (
                    <span className="view-only">View Only</span>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </>
  );
};

export default PlayerList;
