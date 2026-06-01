import React, { useEffect, useState } from 'react';
import injuryService from '../../services/injuryService';
import InjuryForm from './InjuryForm';
import ConfirmDialog from './ConfirmDialog';
import { Injury } from '../../types';
import { useAuth } from '../../context/AuthContext';

const InjuriesTab: React.FC = () => {
  const { user, isAdmin } = useAuth();
  const canManage = user?.role === 'Manager' || isAdmin;

  const [subtab, setSubtab] = useState<'active'|'all'>('active');
  const [injuries, setInjuries] = useState<Injury[]>([]);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<Injury | null>(null);
  const [confirmDelete, setConfirmDelete] = useState<Injury | null>(null);

  const fetch = async () => {
    setLoading(true);
    setError(null);
    try {
      if (subtab === 'active') {
        const items = await injuryService.getActive();
        setInjuries(items);
        setTotalPages(1);
      } else {
        const res = await injuryService.getInjuries(page, pageSize);
        setInjuries(res.items || []);
        setTotalPages(res.pagination?.totalPages || 1);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load injuries');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { void fetch(); }, [subtab, page]);

  const openCreate = () => { setEditing(null); setShowForm(true); };
  const openEdit = (i: Injury) => { setEditing(i); setShowForm(true); };

  const handleSave = async (data: any) => {
    if (editing) {
      await injuryService.updateInjury(editing.id, data);
    } else {
      await injuryService.createInjury(data);
    }
    await fetch();
    setShowForm(false);
  };

  const handleDelete = async (id: number) => {
    try {
      await injuryService.deleteInjury(id);
      setConfirmDelete(null);
      await fetch();
    } catch (err: any) {
      setError(err.message || 'Failed to delete');
    }
  };

  return (
    <div className="tab-panel">
      <div className="subtabs">
        <button className={subtab==='active'? 'active':''} onClick={() => setSubtab('active')}>Active</button>
        <button className={subtab==='all'? 'active':''} onClick={() => setSubtab('all')}>All</button>
      </div>

      <div className="panel-actions">
        <div />
        <div>
          <button className="btn btn-success" onClick={openCreate} disabled={!canManage}>Create Injury</button>
        </div>
      </div>

      {loading && <div className="loading">Loading injuries...</div>}
      {error && <div className="error-message">{error}</div>}

      {!loading && injuries.length === 0 && <div className="empty-state">No injuries found.</div>}

      {injuries.length > 0 && (
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
              {injuries.map((i) => (
                <tr key={i.id} className={i.status === 'Active' ? 'active-row' : ''}>
                  <td>{i.playerName}</td>
                  <td>{i.clubName || '-'}</td>
                  <td>{i.injuryType}</td>
                  <td>{new Date(i.injuryDate).toLocaleDateString()}</td>
                  <td>{i.recoveryDate ? new Date(i.recoveryDate).toLocaleDateString() : '-'}</td>
                  <td><span className={`badge ${i.status === 'Active' ? 'badge-danger' : 'badge-success'}`}>{i.status}</span></td>
                  <td>
                    <button className="btn btn-sm btn-edit" onClick={() => openEdit(i)} disabled={!canManage}>Edit</button>
                    <button className="btn btn-sm btn-delete" onClick={() => setConfirmDelete(i)} disabled={!isAdmin}>Delete</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <div className="pagination">
        <button onClick={() => setPage((p) => Math.max(1, p-1))} disabled={page===1}>Prev</button>
        <span>Page {page} / {totalPages}</span>
        <button onClick={() => setPage((p) => Math.min(totalPages, p+1))} disabled={page===totalPages}>Next</button>
      </div>

      {showForm && <InjuryForm injury={editing} onClose={() => setShowForm(false)} onSubmit={fetch} onSave={handleSave} />}

      {confirmDelete && (
        <ConfirmDialog title="Delete Injury" message={`Delete injury record for ${confirmDelete.playerName}?`} onCancel={() => setConfirmDelete(null)} onConfirm={() => handleDelete(confirmDelete.id)} />
      )}
    </div>
  );
};

export default InjuriesTab;
