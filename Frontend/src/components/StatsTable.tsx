import React from 'react';
import { PlayerStats } from '../types';

interface StatsTableProps {
  stats: PlayerStats[];
  isLoading: boolean;
  onEdit: (stat: PlayerStats) => void;
  onDelete: (id: number) => void;
  canEdit: boolean;
  canDelete: boolean;
}

const StatsTable: React.FC<StatsTableProps> = ({ stats, isLoading, onEdit, onDelete, canEdit, canDelete }) => {
  if (isLoading) {
    return <div className="loading-state">Loading stats...</div>;
  }

  if (stats.length === 0) {
    return <div className="empty-state">No stats available for the selected filters.</div>;
  }

  return (
    <div className="table-container">
      <table className="stats-table">
        <thead>
          <tr>
            <th>Player</th>
            <th>Match Date</th>
            <th>Goals</th>
            <th>Assists</th>
            <th>Yellow</th>
            <th>Red</th>
            <th>Minutes</th>
            <th>Rating</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {stats.map((stat) => (
            <tr key={stat.id}>
              <td>{stat.playerName}</td>
              <td>{new Date(stat.matchDate).toLocaleDateString()}</td>
              <td>{stat.goalsScored}</td>
              <td>{stat.assists}</td>
              <td>{stat.yellowCards}</td>
              <td>{stat.redCards}</td>
              <td>{stat.minutesPlayed}</td>
              <td>{stat.rating !== undefined ? stat.rating : '-'}</td>
              <td className="actions-cell">
                {canEdit && (
                  <button className="btn btn-edit" onClick={() => onEdit(stat)}>
                    Edit
                  </button>
                )}
                {canDelete && (
                  <button className="btn btn-delete" onClick={() => onDelete(stat.id)}>
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

export default StatsTable;
