import React from 'react';
import { Match } from '../types';

interface MatchTableProps {
  matches: Match[];
  isLoading: boolean;
  onEdit: (match: Match) => void;
  onDelete: (id: number) => void;
  userRole?: string;
}

const MatchTable: React.FC<MatchTableProps> = ({ matches, isLoading, onEdit, onDelete, userRole }) => {
  const formatDate = (date: string) => {
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  const getStatusBadgeClass = (status: string) => {
    switch (status?.toLowerCase()) {
      case 'scheduled':
        return 'badge-scheduled';
      case 'ongoing':
        return 'badge-ongoing';
      case 'completed':
        return 'badge-completed';
      default:
        return 'badge-default';
    }
  };

  const isUpcoming = (date: string) => {
    return new Date(date) > new Date();
  };

  if (isLoading) {
    return <div className="loading-spinner">Loading matches...</div>;
  }

  if (matches.length === 0) {
    return <div className="no-data">No matches found</div>;
  }

  return (
    <div className="table-container">
      <table className="matches-table">
        <thead>
          <tr>
            <th>Date</th>
            <th>Home Team</th>
            <th>Away Team</th>
            <th>Score</th>
            <th>Status</th>
            <th>Competition</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {matches.map((match) => (
            <tr key={match.id} className={isUpcoming(match.matchDate) ? 'upcoming' : 'completed'}>
              <td className="date-cell">
                <div>{formatDate(match.matchDate)}</div>
                {match.time && <div className="time">{match.time}</div>}
              </td>
              <td className="team-cell">{match.homeClubName}</td>
              <td className="team-cell">{match.awayClubName}</td>
              <td className="score-cell">
                {match.homeScore !== null && match.awayScore !== null ? (
                  <strong>{match.homeScore} - {match.awayScore}</strong>
                ) : (
                  <span className="no-score">-</span>
                )}
              </td>
              <td className="status-cell">
                <span className={`status-badge ${getStatusBadgeClass(match.status)}`}>
                  {match.status}
                </span>
              </td>
              <td className="competition-cell">{match.competitionType || '-'}</td>
              <td className="actions-cell">
                <button
                  className="btn btn-sm btn-edit"
                  onClick={() => onEdit(match)}
                  title="Edit match"
                >
                  Edit
                </button>
                {userRole === 'Admin' && (
                  <button
                    className="btn btn-sm btn-delete"
                    onClick={() => onDelete(match.id)}
                    title="Delete match"
                  >
                    Delete
                  </button>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default MatchTable;
