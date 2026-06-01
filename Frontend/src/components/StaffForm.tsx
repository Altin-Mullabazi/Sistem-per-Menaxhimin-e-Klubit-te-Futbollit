import React, { useState, useEffect } from 'react';
import { Staff, CreateStaffDto, UpdateStaffDto } from '../types';
import { staffService } from '../services/staffService';
import { clubService } from '../services/clubService';
import apiClient from '../services/apiClient';
import '../styles/Form.css';

interface Props {
  staff: Staff | null;
  onClose: () => void;
  onSubmit: () => void;
}

const StaffForm: React.FC<Props> = ({ staff, onClose, onSubmit }) => {
  const [formData, setFormData] = useState({
    clubId: '',
    userId: '',
    firstName: '',
    lastName: '',
    role: '',
    specialization: '',
    employmentDate: new Date().toISOString().slice(0, 10),
    status: ''
  });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [clubs, setClubs] = useState<{ id: number; name: string }[]>([]);
  const [users, setUsers] = useState<any[]>([]);

  useEffect(() => {
    if (staff) {
      setFormData({
        clubId: staff.clubId?.toString() || '',
        userId: staff.userId || '',
        firstName: staff.firstName,
        lastName: staff.lastName,
        role: staff.role,
        specialization: staff.specialization || '',
        employmentDate: staff.employmentDate.slice(0, 10),
        status: staff.status || ''
      });
    }

    const load = async () => {
      try {
        const c = await clubService.getClubs(1, 200);
        setClubs(c);

        // Load users (admin only endpoint) - try to fetch a page of users
        const resp = await apiClient.get('/users', { params: { page: 1, pageSize: 200 } });
        setUsers(resp.data.data || []);
      } catch (e) {
        // ignore user load failures for non-admins
        console.warn('Could not load users for staff form', e);
      }
    };

    load();
  }, [staff]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const validate = (): boolean => {
    if (!formData.firstName.trim()) { setError('First name is required'); return false; }
    if (!formData.lastName.trim()) { setError('Last name is required'); return false; }
    if (!formData.role.trim()) { setError('Role is required'); return false; }
    if (!formData.clubId) { setError('Club is required'); return false; }
    if (!formData.userId) { setError('User is required'); return false; }
    return true;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    if (!validate()) return;

    setIsLoading(true);
    try {
      if (staff) {
        const payload: UpdateStaffDto = {
          clubId: parseInt(formData.clubId),
          userId: formData.userId,
          firstName: formData.firstName,
          lastName: formData.lastName,
          role: formData.role,
          specialization: formData.specialization || undefined,
          employmentDate: formData.employmentDate || undefined,
          status: formData.status || undefined
        };
        await staffService.updateStaff(staff.id, payload);
      } else {
        const payload: CreateStaffDto = {
          clubId: parseInt(formData.clubId),
          userId: formData.userId,
          firstName: formData.firstName,
          lastName: formData.lastName,
          role: formData.role,
          specialization: formData.specialization || undefined,
          employmentDate: formData.employmentDate,
          status: formData.status || undefined
        };
        await staffService.createStaff(payload);
      }

      onSubmit();
    } catch (err: any) {
      setError(err.message || 'Failed to save staff');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="form-overlay">
      <div className="form-container">
        <div className="form-header">
          <h2>{staff ? 'Edit Staff' : 'Create Staff'}</h2>
          <button className="close-btn" onClick={onClose} disabled={isLoading}>×</button>
        </div>

        {error && <div className="error-message">{error}</div>}

        <form className="player-form" onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Club *</label>
            <select name="clubId" value={formData.clubId} onChange={handleChange} required disabled={isLoading}>
              <option value="">Select a club</option>
              {clubs.map(c => (<option key={c.id} value={c.id}>{c.name}</option>))}
            </select>
          </div>

          <div className="form-group">
            <label>User *</label>
            <select name="userId" value={formData.userId} onChange={handleChange} required disabled={isLoading}>
              <option value="">Select a user</option>
              {users.map((u: any) => (<option key={u.id} value={u.id}>{u.email || u.userName || u.fullName}</option>))}
            </select>
          </div>

          <div className="form-group">
            <label>First Name *</label>
            <input name="firstName" value={formData.firstName} onChange={handleChange} required disabled={isLoading} />
          </div>

          <div className="form-group">
            <label>Last Name *</label>
            <input name="lastName" value={formData.lastName} onChange={handleChange} required disabled={isLoading} />
          </div>

          <div className="form-group">
            <label>Role *</label>
            <input name="role" value={formData.role} onChange={handleChange} required disabled={isLoading} />
          </div>

          <div className="form-group">
            <label>Specialization</label>
            <input name="specialization" value={formData.specialization} onChange={handleChange} disabled={isLoading} />
          </div>

          <div className="form-group">
            <label>Employment Date</label>
            <input type="date" name="employmentDate" value={formData.employmentDate} onChange={handleChange} disabled={isLoading} />
          </div>

          <div className="form-group">
            <label>Status</label>
            <input name="status" value={formData.status} onChange={handleChange} disabled={isLoading} />
          </div>

          <div className="form-actions">
            <button type="button" className="btn btn-secondary" onClick={onClose} disabled={isLoading}>Cancel</button>
            <button type="submit" className="btn btn-primary" disabled={isLoading}>{isLoading ? 'Saving...' : staff ? 'Update Staff' : 'Create Staff'}</button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default StaffForm;
