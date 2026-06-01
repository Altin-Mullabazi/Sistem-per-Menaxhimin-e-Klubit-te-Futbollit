import React from 'react';
import { Club } from '../types';
import '../styles/List.css';

interface ClubListProps {
  clubs: Club[];
  isLoading: boolean;
  onEdit: (club: Club) => void;
  onDelete: (id: number) => void;
  canEdit: boolean;
  canDelete: boolean;
}

const ClubList: React.FC<ClubListProps> = ({ clubs, isLoading, onEdit, onDelete, canEdit, canDelete }) => {
  if (isLoading) {
    return (
      <div className="loading">
        <div className="spinner"></div>
        <p>Loading clubs...</p>
      </div>
    );
  }

  if (clubs.length === 0) {
    return (
      <div className="empty-state">
        <div className="empty-icon">📭</div>
        <p>No clubs found. Try adjusting your search or filters.</p>
      </div>
    );
  }

  return (
    <>
      {/* Desktop Table View */}
      <div className="list-desktop">
        <table className="clubs-table">
          <thead>
            <tr>
              <th>Club Name</th>
              <th>City</th>
              <th>Founded</th>
              <th>President</th>
              <th>Budget</th>
              <th>Players</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {clubs.map((club) => (
              <tr key={club.id} className="club-row">
                <td className="club-name">
                  {club.logoUrl && (
                    <img src={club.logoUrl} alt={club.name} className="club-logo" />
                  )}
                  <span>{club.name}</span>
                </td>
                <td>{club.city}</td>
                <td>{club.foundedYear}</td>
                <td>{club.president || '—'}</td>
                <td>{club.budget ? `$${club.budget.toLocaleString()}` : '—'}</td>
                <td className="player-count">{club.playerCount}</td>
                <td className="actions">
                  {canEdit && (
                    <button
                      className="btn btn-sm btn-secondary"
                      onClick={() => onEdit(club)}
                      title="Edit club"
                    >
                      Edit
                    </button>
                  )}
                  {canDelete && (
                    <button
                      className="btn btn-sm btn-danger"
                      onClick={() => onDelete(club.id)}
                      title="Delete club"
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
          {clubs.map((club) => (
            <div key={club.id} className="club-card">
              {club.logoUrl && (
                <img src={club.logoUrl} alt={club.name} className="card-logo" />
              )}
              <div className="card-content">
                <h3 className="card-title">{club.name}</h3>
                <div className="card-detail">
                  <span className="label">City:</span>
                  <span className="value">{club.city}</span>
                </div>
                <div className="card-detail">
                  <span className="label">Founded:</span>
                  <span className="value">{club.foundedYear}</span>
                </div>
                <div className="card-detail">
                  <span className="label">President:</span>
                  <span className="value">{club.president || '—'}</span>
                </div>
                <div className="card-detail">
                  <span className="label">Budget:</span>
                  <span className="value">{club.budget ? `$${club.budget.toLocaleString()}` : '—'}</span>
                </div>
                <div className="card-detail">
                  <span className="label">Players:</span>
                  <span className="value badge">{club.playerCount}</span>
                </div>
                <div className="card-actions">
                  {canEdit && (
                    <button
                      className="btn btn-sm btn-secondary"
                      onClick={() => onEdit(club)}
                    >
                      Edit
                    </button>
                  )}
                  {canDelete && (
                    <button
                      className="btn btn-sm btn-danger"
                      onClick={() => onDelete(club.id)}
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

export default ClubList;
