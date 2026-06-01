import React from 'react';
import { Contract } from '../../types';

interface ContractsListProps {
  items: Contract[];
  onEdit?: (c: Contract) => void;
  onDelete?: (id: number) => void;
}

const ContractsList: React.FC<ContractsListProps> = ({ items, onEdit, onDelete }) => (
  <div className="table-wrapper">
    <table className="player-table">
      <thead>
        <tr>
          <th>Player</th>
          <th>Club</th>
          <th>Salary</th>
          <th>Start Date</th>
          <th>End Date</th>
          <th>Status</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        {items.map((c) => (
          <tr key={c.id}>
            <td>{c.playerName}</td>
            <td>{c.clubName}</td>
            <td>{c.salary.toFixed(2)}</td>
            <td>{new Date(c.startDate).toLocaleDateString()}</td>
            <td>{new Date(c.endDate).toLocaleDateString()}</td>
            <td>{c.status}</td>
            <td>
              <button className="btn btn-sm btn-edit" onClick={() => onEdit && onEdit(c)}>Edit</button>
              <button className="btn btn-sm btn-delete" onClick={() => onDelete && onDelete(c.id)}>Delete</button>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  </div>
);

export default ContractsList;
