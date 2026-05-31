import React, { useEffect, useState } from 'react';
import { Transfer, CreateTransferDto, UpdateTransferDto } from '../../types';
import { playerService } from '../../services/playerService';

interface TransferFormProps {
  transfer?: Transfer | null;
  onClose: () => void;
  onSubmit: () => void;
  onSave: (data: any) => Promise<void>;
}

const TransferForm: React.FC<TransferFormProps> = ({ transfer, onClose, onSubmit, onSave }) => {
  const [players, setPlayers] = useState<any[]>([]);
  const [form, setForm] = useState<CreateTransferDto>({ playerId: 0, fee: 0, transferDate: '', toClubId: undefined });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    void playerService.getAllPlayers().then(setPlayers).catch(() => setPlayers([]));
  }, []);

  useEffect(() => {
    if (transfer) {
      setForm({
        playerId: transfer.playerId,
        fromClubId: transfer.fromClubId,
        toClubId: transfer.toClubId,
        fee: transfer.fee,
        transferDate: transfer.transferDate.split('T')[0],
      } as CreateTransferDto);
    }
  }, [transfer]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: name === 'fee' ? parseFloat(value || '0') : value } as any));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await onSave(form as any);
      onSubmit();
    } catch (err: any) {
      setError(err.message || 'Failed to save transfer');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="form-overlay">
      <div className="form-container">
        <div className="form-header">
          <h2>{transfer ? 'Edit Transfer' : 'Create Transfer'}</h2>
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
            <label>To Club ID</label>
            <input name="toClubId" type="number" value={form.toClubId as any || ''} onChange={handleChange} />
          </div>

          <div className="form-group">
            <label>Fee</label>
            <input name="fee" type="number" step="0.01" value={form.fee as any} onChange={handleChange} required />
          </div>

          <div className="form-group">
            <label>Transfer Date</label>
            <input name="transferDate" type="date" value={form.transferDate as any} onChange={handleChange} required />
          </div>

          <div className="form-actions">
            <button type="button" className="btn btn-secondary" onClick={onClose} disabled={loading}>Cancel</button>
            <button type="submit" className="btn btn-primary" disabled={loading}>{loading ? 'Saving...' : transfer ? 'Update' : 'Create'}</button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default TransferForm;
