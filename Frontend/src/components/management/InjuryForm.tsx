import React, { useEffect, useState } from 'react';
import { Injury, CreateInjuryDto } from '../../types';
import { playerService } from '../../services/playerService';

interface InjuryFormProps {
  injury?: Injury | null;
  onClose: () => void;
  onSubmit: () => void;
  onSave: (data: any) => Promise<void>;
}

const InjuryForm: React.FC<InjuryFormProps> = ({ injury, onClose, onSubmit, onSave }) => {
  const [players, setPlayers] = useState<any[]>([]);
  const [form, setForm] = useState<CreateInjuryDto>({ playerId: 0, injuryType: '', injuryDate: '', recoveryDate: '' });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    void playerService.getAllPlayers().then(setPlayers).catch(() => setPlayers([]));
  }, []);

  useEffect(() => {
    if (injury) {
      setForm({ playerId: injury.playerId, injuryType: injury.injuryType, injuryDate: injury.injuryDate.split('T')[0], recoveryDate: injury.recoveryDate ? injury.recoveryDate.split('T')[0] : '' });
    }
  }, [injury]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value } as any));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await onSave(form as any);
      onSubmit();
    } catch (err: any) {
      setError(err.message || 'Failed to save injury');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="form-overlay">
      <div className="form-container">
        <div className="form-header">
          <h2>{injury ? 'Edit Injury' : 'Create Injury'}</h2>
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
            <label>Injury Type</label>
            <input name="injuryType" value={form.injuryType} onChange={handleChange} required />
          </div>

          <div className="form-group">
            <label>Injury Date</label>
            <input name="injuryDate" type="date" value={form.injuryDate as any} onChange={handleChange} required />
          </div>

          <div className="form-group">
            <label>Recovery Date</label>
            <input name="recoveryDate" type="date" value={form.recoveryDate as any} onChange={handleChange} />
          </div>

          <div className="form-actions">
            <button type="button" className="btn btn-secondary" onClick={onClose} disabled={loading}>Cancel</button>
            <button type="submit" className="btn btn-primary" disabled={loading}>{loading ? 'Saving...' : injury ? 'Update' : 'Create'}</button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default InjuryForm;
