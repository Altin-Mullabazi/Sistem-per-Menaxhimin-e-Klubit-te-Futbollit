import React from 'react';
import { Injury } from '../../types';

interface InjuriesListProps {
  items: Injury[];
  onEdit?: (i: Injury) => void;
  onDelete?: (id: number) => void;
}

const InjuriesList: React.FC<InjuriesListProps> = ({ items, onEdit, onDelete }) => (
  <div className="table-wrapper">
    <table className="player-table">
      <thead>
        <tr>
          <th>Player</th>
          <th>Club</th>
          <th>Injury Type</th>
          <th>Injury Date</th>
          <th>Recovery Date</th>
          <th>Status</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        {items.map((i) => (
          <tr key={i.id}>
            <td>{i.playerName}</td>
            <td>{i.clubName || '-'}</td>
            <td>{i.injuryType}</td>
            <td>{new Date(i.injuryDate).toLocaleDateString()}</td>
            <td>{i.recoveryDate ? new Date(i.recoveryDate).toLocaleDateString() : '-'}</td>
            <td>{i.status}</td>
            <td>
              <button className="btn btn-sm btn-edit" onClick={() => onEdit && onEdit(i)}>Edit</button>
              <button className="btn btn-sm btn-delete" onClick={() => onDelete && onDelete(i.id)}>Delete</button>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  </div>
);

export default InjuriesList;
