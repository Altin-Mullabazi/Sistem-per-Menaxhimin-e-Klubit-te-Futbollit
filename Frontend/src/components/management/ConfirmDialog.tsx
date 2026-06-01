import React from 'react';

interface ConfirmDialogProps {
  title?: string;
  message: string;
  onConfirm: () => void;
  onCancel: () => void;
  confirmLabel?: string;
}

const ConfirmDialog: React.FC<ConfirmDialogProps> = ({ title, message, onConfirm, onCancel, confirmLabel = 'Delete' }) => {
  return (
    <div className="form-overlay">
      <div className="form-container small">
        <div className="form-header">
          <h3>{title || 'Confirm'}</h3>
          <button className="close-btn" onClick={onCancel}>×</button>
        </div>
        <div className="form-body">
          <p>{message}</p>
        </div>
        <div className="form-actions">
          <button className="btn btn-secondary" onClick={onCancel}>Cancel</button>
          <button className="btn btn-danger" onClick={onConfirm}>{confirmLabel}</button>
        </div>
      </div>
    </div>
  );
};

export default ConfirmDialog;
