import React, { useEffect, useState } from 'react';
import { Contract, CreateContractDto } from '../../types';
import { playerService } from '../../services/playerService';

interface ContractFormProps {
  contract?: Contract | null;
  onClose: () => void;
  onSubmit: () => void;
  onSave: (data: any) => Promise<void>;
}

const ContractForm: React.FC<ContractFormProps> = ({ contract, onClose, onSubmit, onSave }) => {
  const [players, setPlayers] = useState<any[]>([]);
  const [form, setForm] = useState<CreateContractDto>({ playerId: 0, clubId: 0, salary: 0, startDate: '', endDate: '' });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    void playerService.getAllPlayers().then(setPlayers).catch(() => setPlayers([]));
  }, []);

  useEffect(() => {
    if (contract) {
      setForm({ playerId: contract.playerId, clubId: contract.clubId, salary: contract.salary, startDate: contract.startDate.split('T')[0], endDate: contract.endDate.split('T')[0] });
    }
  }, [contract]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: name === 'salary' ? parseFloat(value || '0') : value } as any));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await onSave(form as any);
      onSubmit();
    } catch (err: any) {
      setError(err.message || 'Failed to save contract');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="form-overlay">
      <div className="form-container">
        <div className="form-header">
          <h2>{contract ? 'Edit Contract' : 'Create Contract'}</h2>
          <button className="close-btn" onClick={onClose} disabled={loading}>×</button>
        </div>

        {error && <div className="error-message">{error}</div>}

        <form onSubmit={handleSubmit} className="player-form">
          <div className="form-group">
            <label>Player</label>
            <select name="playerId" value={form.playerId} onChange={handleChange} required>
              <option value={0}>Select player</option>
              {players.map((p) => (
                <option key={p.id} value={p.id}>{p.firstName} {p.lastName}</option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label>Club ID</label>
            <input name="clubId" type="number" value={form.clubId as any || ''} onChange={handleChange} required />
          </div>

          <div className="form-group">
            <label>Salary</label>
            <input name="salary" type="number" step="0.01" value={form.salary as any} onChange={handleChange} required />
          </div>

          <div className="form-group">
            <label>Start Date</label>
            <input name="startDate" type="date" value={form.startDate as any} onChange={handleChange} required />
          </div>

          <div className="form-group">
            <label>End Date</label>
            <input name="endDate" type="date" value={form.endDate as any} onChange={handleChange} required />
          </div>

          <div className="form-actions">
            <button type="button" className="btn btn-secondary" onClick={onClose} disabled={loading}>Cancel</button>
            <button type="submit" className="btn btn-primary" disabled={loading}>{loading ? 'Saving...' : contract ? 'Update' : 'Create'}</button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default ContractForm;
