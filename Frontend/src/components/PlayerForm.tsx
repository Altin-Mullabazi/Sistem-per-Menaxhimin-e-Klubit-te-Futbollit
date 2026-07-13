import React, { useState, useEffect } from 'react';
import { Player, Club } from '../types';
import { playerService } from '../services/playerService';
import Modal from './Modal';
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
    const jersey = parseInt(formData.jerseyNumber, 10);
    if (isNaN(jersey) || jersey < 1 || jersey > 99) {
      setError('Jersey number is required (1–99)');
      return false;
    }
    if (clubs.length > 0 && !formData.clubId) {
      setError('Please select a club');
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
      const jerseyNumber = parseInt(formData.jerseyNumber, 10);
      const payload = {
        firstName: formData.firstName,
        lastName: formData.lastName,
        age: parseInt(formData.age, 10),
        position: formData.position,
        clubId: formData.clubId ? parseInt(formData.clubId, 10) : undefined,
        jerseyNumber,
      };

      if (player) {
        await playerService.updatePlayer(player.id, payload);
      } else {
        await playerService.createPlayer(payload);
      }
      onSubmit();
    } catch (err: any) {
      setError(err.message || 'Failed to save player');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Modal title={player ? 'Edit Player' : 'Create New Player'} onClose={onClose}>
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
              <label htmlFor="jerseyNumber">Jersey Number *</label>
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
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="clubId">Club{clubs.length > 0 ? ' *' : ''}</label>
              <select
                id="clubId"
                name="clubId"
                value={formData.clubId}
                onChange={handleChange}
                disabled={isLoading || clubs.length === 0}
              >
                <option value="">
                  {clubs.length === 0 ? 'No clubs — create one under Clubs first' : 'Select club'}
                </option>
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
    </Modal>
  );
};

export default PlayerForm;
