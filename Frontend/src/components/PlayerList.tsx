import React from 'react';
import { Player } from '../types';
import '../styles/PlayerList.css';

interface PlayerListProps {
  players: Player[];
  onEdit: (player: Player) => void;
  onDelete: (id: number) => void;
}

const PlayerList: React.FC<PlayerListProps> = ({ players, onEdit, onDelete }) => {
  return (
    <div className="player-list-container">
      <div className="table-wrapper">
        <table className="player-table">
          <thead>
            <tr>
              <th>First Name</th>
              <th>Last Name</th>
              <th>Age</th>
              <th>Position</th>
              <th>Club</th>
              <th>Created</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {players.map((player) => (
              <tr key={player.id} className="player-row">
                <td>{player.firstName}</td>
                <td>{player.lastName}</td>
                <td>{player.age}</td>
                <td>
                  <span className="position-badge">{player.position}</span>
                </td>
                <td>{player.clubName || '-'}</td>
                <td>{new Date(player.createdAt).toLocaleDateString()}</td>
                <td className="actions-cell">
                  <button
                    className="btn btn-sm btn-edit"
                    onClick={() => onEdit(player)}
                    title="Edit player"
                  >
                    ✏️
                  </button>
                  <button
                    className="btn btn-sm btn-delete"
                    onClick={() => onDelete(player.id)}
                    title="Delete player"
                  >
                    🗑️
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="list-footer">
        <p>Total Players: <strong>{players.length}</strong></p>
      </div>
    </div>
  );
};

export default PlayerList;
