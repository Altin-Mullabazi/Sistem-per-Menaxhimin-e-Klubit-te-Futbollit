import React, { useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { Staff } from '../types';
import { staffService } from '../services/staffService';
import StaffList from '../components/StaffList';
import StaffForm from '../components/StaffForm';
import '../styles/Players.css';

const StaffPage: React.FC = () => {
  const { user } = useAuth();
  const [staff, setStaff] = useState<Staff[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<Staff | null>(null);

  const loadStaff = async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await staffService.getStaff(1, 100);
      setStaff(res.staff);
    } catch (err: any) {
      setError(err.message || 'Failed to load staff');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { loadStaff(); }, []);

  const onCreate = () => { setEditing(null); setShowForm(true); };
  const onEdit = (s: Staff) => { setEditing(s); setShowForm(true); };
  const onClose = () => { setShowForm(false); setEditing(null); };
  const onSubmit = async () => { await loadStaff(); onClose(); };

  const onDelete = async (id: number) => {
    if (!window.confirm('Delete this staff member?')) return;
    try {
      await staffService.deleteStaff(id);
      setStaff(staff.filter(s => s.id !== id));
    } catch (err: any) { setError(err.message || 'Failed to delete staff'); }
  };

  return (
    <div className="players-container">
      <div className="players-header">
        <h1>Staff Management</h1>
        <div className="header-info"><p>Logged in as: <strong>{user?.username}</strong></p></div>
      </div>

      {error && <div className="error-message">{error}</div>}

      <button className="btn btn-primary" onClick={onCreate} disabled={loading}>+ Add Staff</button>

      {showForm && <StaffForm staff={editing} onClose={onClose} onSubmit={onSubmit} />}

      {loading && <div className="loading">Loading staff...</div>}

      {!loading && staff.length === 0 && <div className="empty-state"><p>No staff found.</p></div>}

      {!loading && staff.length > 0 && <StaffList staff={staff} onEdit={onEdit} onDelete={onDelete} />}
    </div>
  );
};

export default StaffPage;
