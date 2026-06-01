import React from 'react';
import { Transfer } from '../../types';

interface TransfersListProps {
  items: Transfer[];
  onEdit?: (t: Transfer) => void;
  onDelete?: (id: number) => void;
}

const TransfersList: React.FC<TransfersListProps> = ({ items, onEdit, onDelete }) => {
  return (
    <div className="table-wrapper">
      <table className="player-table">
        <thead>
          <tr>
            <th>Player</th>
            <th>From Club</th>
            <th>To Club</th>
            <th>Fee</th>
            <th>Transfer Date</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {items.map((t) => (
            <tr key={t.id}>
              <td>{t.playerName}</td>
              <td>{t.fromClubName || '-'}</td>
              <td>{t.toClubName || '-'}</td>
              <td>{t.fee.toFixed(2)}</td>
              <td>{new Date(t.transferDate).toLocaleDateString()}</td>
              <td>
                <button className="btn btn-sm btn-edit" onClick={() => onEdit && onEdit(t)}>Edit</button>
                <button className="btn btn-sm btn-delete" onClick={() => onDelete && onDelete(t.id)}>Delete</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default TransfersList;
