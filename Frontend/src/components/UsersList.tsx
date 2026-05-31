import React from 'react';
import { User } from '../types';
import '../styles/PlayerList.css';
import '../styles/UsersPage.css';

interface Props {
  users: User[];
  onEdit: (user: User) => void;
  onDelete: (user: User) => void;
}

const UsersList: React.FC<Props> = ({ users, onEdit, onDelete }) => {
  return (
    <div className="player-list-container">
      <div className="users-mobile-list">
        {users.map((user) => (
          <article key={user.id} className="users-mobile-card">
            <div className="users-mobile-card-header">
              <div>
                <strong>{user.firstName || '-'} {user.lastName || ''}</strong>
                <span>{user.email}</span>
              </div>
              <span className="users-mobile-role">{user.role || '-'}</span>
            </div>
            <dl className="users-mobile-meta">
              <div>
                <dt>Created</dt>
                <dd>{user.createdAt ? new Date(user.createdAt).toLocaleDateString() : '-'}</dd>
              </div>
            </dl>
            <div className="users-mobile-actions">
              <button className="btn btn-sm btn-edit" onClick={() => onEdit(user)} title="Edit user">Edit</button>
              <button className="btn btn-sm btn-delete" onClick={() => onDelete(user)} title="Delete user">Delete</button>
            </div>
          </article>
        ))}
      </div>

      <div className="table-wrapper">
        <table className="player-table">
          <thead>
            <tr>
              <th>Email</th>
              <th>First Name</th>
              <th>Last Name</th>
              <th>Roles</th>
              <th>Created At</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {users.map((u) => (
              <tr key={u.id} className="player-row">
                <td>{u.email}</td>
                <td>{u.firstName}</td>
                <td>{u.lastName}</td>
                <td>{u.role || '-'}</td>
                <td>{u.createdAt ? new Date(u.createdAt).toLocaleString() : '-'}</td>
                <td className="actions-cell">
                  <button className="btn btn-sm btn-edit" onClick={() => onEdit(u)} title="Edit user">Edit</button>
                  <button className="btn btn-sm btn-delete" onClick={() => onDelete(u)} title="Delete user">Delete</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="list-footer">
        <p>Total Users: <strong>{users.length}</strong></p>
      </div>
    </div>
  );
};

export default UsersList;
