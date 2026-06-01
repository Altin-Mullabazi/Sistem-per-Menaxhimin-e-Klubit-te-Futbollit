import React from 'react';
import { Staff } from '../types';
import '../styles/PlayerList.css';

interface Props {
  staff: Staff[];
  onEdit: (s: Staff) => void;
  onDelete: (id: number) => void;
}

const StaffList: React.FC<Props> = ({ staff, onEdit, onDelete }) => {
  return (
    <div className="player-list-container">
      <div className="table-wrapper">
        <table className="player-table">
          <thead>
            <tr>
              <th>First Name</th>
              <th>Last Name</th>
              <th>Role</th>
              <th>Specialization</th>
              <th>Club</th>
              <th>Employed</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {staff.map((s) => (
              <tr key={s.id} className="player-row">
                <td>{s.firstName}</td>
                <td>{s.lastName}</td>
                <td>{s.role}</td>
                <td>{s.specialization || '-'}</td>
                <td>{s.clubName || '-'}</td>
                <td>{new Date(s.employmentDate).toLocaleDateString()}</td>
                <td>{s.status || '-'}</td>
                <td className="actions-cell">
                  <button className="btn btn-sm btn-edit" onClick={() => onEdit(s)}>✏️</button>
                  <button className="btn btn-sm btn-delete" onClick={() => onDelete(s.id)}>🗑️</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="list-footer">
        <p>Total Staff: <strong>{staff.length}</strong></p>
      </div>
    </div>
  );
};

export default StaffList;
