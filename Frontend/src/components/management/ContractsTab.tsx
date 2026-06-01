import React, { useEffect, useState } from 'react';
import contractService from '../../services/contractService';
import ContractForm from './ContractForm';
import ConfirmDialog from './ConfirmDialog';
import { Contract } from '../../types';
import { useAuth } from '../../context/AuthContext';

const ContractsTab: React.FC = () => {
  const { user, isAdmin } = useAuth();
  const canManage = user?.role === 'Manager' || isAdmin;

  const [subtab, setSubtab] = useState<'active'|'expiring'|'all'>('active');
  const [contracts, setContracts] = useState<Contract[]>([]);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<Contract | null>(null);
  const [confirmDelete, setConfirmDelete] = useState<Contract | null>(null);

  const fetch = async () => {
    setLoading(true);
    setError(null);
    try {
      if (subtab === 'active') {
        const items = await contractService.getActive();
        setContracts(items);
        setTotalPages(1);
      } else if (subtab === 'expiring') {
        const items = await contractService.getExpiring();
        setContracts(items);
        setTotalPages(1);
      } else {
        const res = await contractService.getContracts(page, pageSize);
        setContracts(res.items || []);
        setTotalPages(res.pagination?.totalPages || 1);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load contracts');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { void fetch(); }, [subtab, page]);

  const openCreate = () => { setEditing(null); setShowForm(true); };
  const openEdit = (c: Contract) => { setEditing(c); setShowForm(true); };

  const handleSave = async (data: any) => {
    if (editing) {
      await contractService.updateContract(editing.id, data);
    } else {
      await contractService.createContract(data);
    }
    await fetch();
    setShowForm(false);
  };

  const handleDelete = async (id: number) => {
    try {
      await contractService.deleteContract(id);
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
        <button className={subtab==='expiring'? 'active':''} onClick={() => setSubtab('expiring')}>Expiring</button>
        <button className={subtab==='all'? 'active':''} onClick={() => setSubtab('all')}>All</button>
      </div>

      <div className="panel-actions">
        <div />
        <div>
          <button className="btn btn-success" onClick={openCreate} disabled={!canManage}>Create Contract</button>
        </div>
      </div>

      {loading && <div className="loading">Loading contracts...</div>}
      {error && <div className="error-message">{error}</div>}

      {!loading && contracts.length === 0 && <div className="empty-state">No contracts found.</div>}

      {contracts.length > 0 && (
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
              {contracts.map((c) => {
                const daysRemaining = c.daysRemaining ?? Math.ceil((new Date(c.endDate).getTime() - Date.now()) / (1000*60*60*24));
                const isExpiring = daysRemaining < 90;
                return (
                  <tr key={c.id} className={isExpiring ? 'expiring-row' : ''}>
                    <td>{c.playerName}</td>
                    <td>{c.clubName}</td>
                    <td>{c.salary.toFixed(2)}</td>
                    <td>{new Date(c.startDate).toLocaleDateString()}</td>
                    <td>{new Date(c.endDate).toLocaleDateString()}</td>
                    <td>{isExpiring ? <span className="badge badge-warning">Expiring</span> : <span className="badge badge-info">{c.status}</span>}</td>
                    <td>
                      <button className="btn btn-sm btn-edit" onClick={() => openEdit(c)} disabled={!canManage}>Edit</button>
                      <button className="btn btn-sm btn-delete" onClick={() => setConfirmDelete(c)} disabled={!isAdmin}>Delete</button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}

      <div className="pagination">
        <button onClick={() => setPage((p) => Math.max(1, p-1))} disabled={page===1}>Prev</button>
        <span>Page {page} / {totalPages}</span>
        <button onClick={() => setPage((p) => Math.min(totalPages, p+1))} disabled={page===totalPages}>Next</button>
      </div>

      {showForm && <ContractForm contract={editing} onClose={() => setShowForm(false)} onSubmit={fetch} onSave={handleSave} />}

      {confirmDelete && (
        <ConfirmDialog title="Delete Contract" message={`Delete contract of ${confirmDelete.playerName}?`} onCancel={() => setConfirmDelete(null)} onConfirm={() => handleDelete(confirmDelete.id)} />
      )}
    </div>
  );
};

export default ContractsTab;
