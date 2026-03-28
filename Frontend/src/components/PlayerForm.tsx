import React, { useState, useEffect } from 'react';
import { Player, CreatePlayerDto, UpdatePlayerDto } from '../types';
import { playerService } from '../services/playerService';
import '../styles/Form.css';

interface PlayerFormProps {
  player: Player | null;
  onClose: () => void;
  onSubmit: () => void;
}

const PlayerForm: React.FC<PlayerFormProps> = ({ player, onClose, onSubmit }) => {
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    age: '',
    position: '',
    clubName: '',
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
        clubName: player.clubName || '',
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
          clubName: formData.clubName || undefined,
        };
        await playerService.updatePlayer(player.id, updateData);
      } else {
        const createData: CreatePlayerDto = {
          firstName: formData.firstName,
          lastName: formData.lastName,
          age: parseInt(formData.age),
          position: formData.position,
          clubName: formData.clubName || undefined,
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
    <div className="form-overlay">
      <div className="form-container">
        <div className="form-header">
          <h2>{player ? 'Edit Player' : 'Add New Player'}</h2>
          <button className="close-btn" onClick={onClose} disabled={isLoading}>
            ×
          </button>
        </div>

        {error && <div className="error-message">{error}</div>}

        <form onSubmit={handleSubmit} className="player-form">
          <div className="form-group">
            <label htmlFor="firstName">First Name *</label>
            <input
              id="firstName"
              type="text"
              name="firstName"
              value={formData.firstName}
              onChange={handleChange}
              placeholder="Enter first name"
              required
              disabled={isLoading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="lastName">Last Name *</label>
            <input
              id="lastName"
              type="text"
              name="lastName"
              value={formData.lastName}
              onChange={handleChange}
              placeholder="Enter last name"
              required
              disabled={isLoading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="age">Age *</label>
            <input
              id="age"
              type="number"
              name="age"
              value={formData.age}
              onChange={handleChange}
              placeholder="Enter age (16-45)"
              min="16"
              max="45"
              required
              disabled={isLoading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="position">Position *</label>
            <select
              id="position"
              name="position"
              value={formData.position}
              onChange={handleChange}
              required
              disabled={isLoading}
            >
              <option value="">Select a position</option>
              {positions.map((pos) => (
                <option key={pos} value={pos}>
                  {pos}
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label htmlFor="clubName">Club Name</label>
            <input
              id="clubName"
              type="text"
              name="clubName"
              value={formData.clubName}
              onChange={handleChange}
              placeholder="Enter club name (optional)"
              disabled={isLoading}
            />
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
