import React from 'react';
import '../styles/UsersPage.css';

interface Props {
  message: string;
  onConfirm: () => void;
  onCancel: () => void;
  loading?: boolean;
}

const DeleteConfirmation: React.FC<Props> = ({ message, onConfirm, onCancel, loading }) => {
  return (
    <div className="users-modal-overlay" role="presentation" onClick={onCancel}>
      <div className="users-modal" role="dialog" aria-modal="true" aria-label="Delete user dialog" onClick={(event) => event.stopPropagation()}>
        <div className="users-modal-header">
          <div>
            <h3>Confirm Delete</h3>
            <p className="users-danger-text">This action cannot be undone.</p>
          </div>
          <button type="button" className="users-modal-close" onClick={onCancel} aria-label="Close dialog">×</button>
        </div>

        <div className="users-confirmation">
          <div className="users-confirmation-accent">{message}</div>
          <p>All related user access will be removed immediately.</p>
          <div className="users-confirmation-actions">
            <button className="btn" onClick={onCancel} disabled={loading}>Cancel</button>
            <button className="btn btn-danger" onClick={onConfirm} disabled={loading}>{loading ? 'Deleting...' : 'Confirm'}</button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default DeleteConfirmation;
