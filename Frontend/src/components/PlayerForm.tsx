import React, { useState, useEffect } from 'react';
import { Player, CreatePlayerDto, UpdatePlayerDto, Club } from '../types';
import { playerService } from '../services/playerService';
import '../styles/Form.css';

interface PlayerFormProps {
  player: Player | null;
  onClose: () => void;
  onSubmit: () => void;
  clubs?: Club[];
}

const PlayerForm: React.FC<PlayerFormProps> = ({ player, onClose, onSubmit, clubs = [] }) => {
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    age: '',
    position: '',
    clubId: '',
    jerseyNumber: '',
  });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const positions = ['Goalkeeper', 'Defender', 'Midfielder', 'Forward', 'Left Winger', 'Right Winger'];

  useEffect(() => {
    if (player) {
      setFormData({
        firstName: player.firstName,
        lastName: player.lastName,
        age: player.age.toString(),
        position: player.position,
        clubId: player.clubId?.toString() || '',
        jerseyNumber: player.jerseyNumber?.toString() || '',
      });
    }
  }, [player]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const validateForm = (): boolean => {
    if (!formData.firstName.trim()) {
      setError('First name is required');
      return false;
    }
    if (!formData.lastName.trim()) {
      setError('Last name is required');
      return false;
    }
    const age = parseInt(formData.age);
    if (isNaN(age) || age < 16 || age > 45) {
      setError('Age must be between 16 and 45');
      return false;
    }
    if (!formData.position.trim()) {
      setError('Position is required');
      return false;
    }
    if (formData.jerseyNumber && isNaN(parseInt(formData.jerseyNumber))) {
      setError('Jersey number must be a valid number');
      return false;
    }
    return true;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!validateForm()) {
      return;
    }

    setIsLoading(true);
    try {
      if (player) {
        const updateData: UpdatePlayerDto = {
          firstName: formData.firstName,
          lastName: formData.lastName,
          age: parseInt(formData.age),
          position: formData.position,
          clubId: formData.clubId ? parseInt(formData.clubId) : undefined,
          jerseyNumber: formData.jerseyNumber ? parseInt(formData.jerseyNumber) : undefined,
        };
        await playerService.updatePlayer(player.id, updateData);
      } else {
        const createData: CreatePlayerDto = {
          firstName: formData.firstName,
          lastName: formData.lastName,
          age: parseInt(formData.age),
          position: formData.position,
          clubId: formData.clubId ? parseInt(formData.clubId) : undefined,
          jerseyNumber: formData.jerseyNumber ? parseInt(formData.jerseyNumber) : undefined,
        };
        await playerService.createPlayer(createData);
      }
      onSubmit();
    } catch (err: any) {
      setError(err.message || 'Failed to save player');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{player ? 'Edit Player' : 'Create New Player'}</h2>
          <button className="close-btn" onClick={onClose}>✕</button>
        </div>

        <form onSubmit={handleSubmit} className="form">
          {error && <div className="error-message">{error}</div>}

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="firstName">First Name *</label>
              <input
                type="text"
                id="firstName"
                name="firstName"
                value={formData.firstName}
                onChange={handleChange}
                placeholder="Enter first name"
                disabled={isLoading}
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="lastName">Last Name *</label>
              <input
                type="text"
                id="lastName"
                name="lastName"
                value={formData.lastName}
                onChange={handleChange}
                placeholder="Enter last name"
                disabled={isLoading}
                required
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="age">Age *</label>
              <input
                type="number"
                id="age"
                name="age"
                value={formData.age}
                onChange={handleChange}
                min="16"
                max="45"
                disabled={isLoading}
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="position">Position *</label>
              <select
                id="position"
                name="position"
                value={formData.position}
                onChange={handleChange}
                disabled={isLoading}
                required
              >
                <option value="">Select position</option>
                {positions.map((pos) => (
                  <option key={pos} value={pos}>
                    {pos}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="jerseyNumber">Jersey Number (optional)</label>
              <input
                type="number"
                id="jerseyNumber"
                name="jerseyNumber"
                value={formData.jerseyNumber}
                onChange={handleChange}
                placeholder="Enter jersey number"
                min="1"
                max="99"
                disabled={isLoading}
              />
            </div>

            <div className="form-group">
              <label htmlFor="clubId">Club (optional)</label>
              <select
                id="clubId"
                name="clubId"
                value={formData.clubId}
                onChange={handleChange}
                disabled={isLoading}
              >
                <option value="">Select club</option>
                {clubs.map((club) => (
                  <option key={club.id} value={club.id}>
                    {club.name}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="form-actions">
            <button type="button" className="btn btn-secondary" onClick={onClose} disabled={isLoading}>
              Cancel
            </button>
            <button type="submit" className="btn btn-primary" disabled={isLoading}>
              {isLoading ? 'Saving...' : player ? 'Update Player' : 'Create Player'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default PlayerForm;
