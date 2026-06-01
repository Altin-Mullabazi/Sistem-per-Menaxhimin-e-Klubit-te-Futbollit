import React, { useEffect, useState } from 'react';
import transferService from '../../services/transferService';
import TransferForm from './TransferForm';
import ConfirmDialog from './ConfirmDialog';
import { Transfer } from '../../types';
import { useAuth } from '../../context/AuthContext';

const TransfersTab: React.FC = () => {
  const { isAuthenticated, user, isAdmin } = useAuth();
  const canManage = user?.role === 'Manager' || isAdmin;

  const [transfers, setTransfers] = useState<Transfer[]>([]);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<Transfer | null>(null);
  const [confirmDelete, setConfirmDelete] = useState<Transfer | null>(null);

  const fetch = async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await transferService.getTransfers({ page, pageSize });
      setTransfers(res.items || []);
      setTotalPages(res.pagination?.totalPages || 1);
    } catch (err: any) {
      setError(err.message || 'Failed to load transfers');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { void fetch(); }, [page]);

  const handleCreate = () => {
    setEditing(null);
    setShowForm(true);
  };

  const handleEdit = (t: Transfer) => {
    setEditing(t);
    setShowForm(true);
  };

  const handleSave = async (data: any) => {
    if (editing) {
      await transferService.updateTransfer(editing.id, data);
    } else {
      await transferService.createTransfer(data);
    }
    await fetch();
    setShowForm(false);
  };

  const handleDelete = async (id: number) => {
    try {
      await transferService.deleteTransfer(id);
      setConfirmDelete(null);
      await fetch();
    } catch (err: any) {
      setError(err.message || 'Failed to delete');
    }
  };

  return (
    <div className="tab-panel">
      <div className="panel-actions">
        <div>
          <input placeholder="Search player" className="search-input" />
        </div>
        <div>
          <button className="btn btn-success" onClick={handleCreate} disabled={!canManage}>Create Transfer</button>
        </div>
      </div>

      {loading && <div className="loading">Loading transfers...</div>}
      {error && <div className="error-message">{error}</div>}

      {!loading && transfers.length === 0 && <div className="empty-state">No transfers found.</div>}

      {transfers.length > 0 && (
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
              {transfers.map((t) => (
                <tr key={t.id}>
                  <td>{t.playerName}</td>
                  <td>{t.fromClubName || '-'}</td>
                  <td>{t.toClubName || '-'}</td>
                  <td>{t.fee.toFixed(2)}</td>
                  <td>{new Date(t.transferDate).toLocaleDateString()}</td>
                  <td>
                    <button className="btn btn-sm btn-edit" onClick={() => handleEdit(t)} disabled={!canManage}>Edit</button>
                    <button className="btn btn-sm btn-delete" onClick={() => setConfirmDelete(t)} disabled={!isAdmin}>Delete</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <div className="pagination">
        <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1}>Prev</button>
        <span>Page {page} / {totalPages}</span>
        <button onClick={() => setPage((p) => Math.min(totalPages, p + 1))} disabled={page === totalPages}>Next</button>
      </div>

      {showForm && (
        <TransferForm transfer={editing} onClose={() => setShowForm(false)} onSubmit={fetch} onSave={handleSave} />
      )}

      {confirmDelete && (
        <ConfirmDialog
          title="Delete Transfer"
          message={`Delete transfer of ${confirmDelete.playerName}?`}
          onCancel={() => setConfirmDelete(null)}
          onConfirm={() => handleDelete(confirmDelete.id)}
        />
      )}
    </div>
  );
};

export default TransfersTab;
