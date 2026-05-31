import React, { useState, useEffect } from 'react';
import { Club, CreateClubDto, UpdateClubDto } from '../types';
import { clubService } from '../services/clubService';
import '../styles/Form.css';

interface ClubFormProps {
  club: Club | null;
  onClose: () => void;
  onSubmit: () => void;
}

const ClubForm: React.FC<ClubFormProps> = ({ club, onClose, onSubmit }) => {
  const [formData, setFormData] = useState({
    name: '',
    city: '',
    logoUrl: '',
    foundedYear: new Date().getFullYear(),
    president: '',
    budget: '',
  });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (club) {
      setFormData({
        name: club.name,
        city: club.city,
        logoUrl: club.logoUrl || '',
        foundedYear: club.foundedYear,
        president: club.president || '',
        budget: club.budget?.toString() || '',
      });
    }
  }, [club]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const validateForm = (): boolean => {
    if (!formData.name.trim()) {
      setError('Club name is required');
      return false;
    }
    if (!formData.city.trim()) {
      setError('City is required');
      return false;
    }
    const year = parseInt(formData.foundedYear.toString());
    if (isNaN(year) || year < 1800 || year > new Date().getFullYear()) {
      setError(`Founded year must be between 1800 and ${new Date().getFullYear()}`);
      return false;
    }
    if (formData.budget && isNaN(parseFloat(formData.budget))) {
      setError('Budget must be a valid number');
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
      if (club) {
        const updateData: UpdateClubDto = {
          name: formData.name,
          city: formData.city,
          logoUrl: formData.logoUrl || undefined,
          foundedYear: parseInt(formData.foundedYear.toString()),
          president: formData.president || undefined,
          budget: formData.budget ? parseFloat(formData.budget) : undefined,
        };
        await clubService.updateClub(club.id, updateData);
      } else {
        const createData: CreateClubDto = {
          name: formData.name,
          city: formData.city,
          logoUrl: formData.logoUrl || undefined,
          foundedYear: parseInt(formData.foundedYear.toString()),
          president: formData.president || undefined,
          budget: formData.budget ? parseFloat(formData.budget) : undefined,
        };
        await clubService.createClub(createData);
      }
      onSubmit();
    } catch (err: any) {
      setError(err.message || 'Failed to save club');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{club ? 'Edit Club' : 'Create New Club'}</h2>
          <button className="close-btn" onClick={onClose}>✕</button>
        </div>

        <form onSubmit={handleSubmit} className="form">
          {error && <div className="error-message">{error}</div>}

          <div className="form-group">
            <label htmlFor="name">Club Name *</label>
            <input
              type="text"
              id="name"
              name="name"
              value={formData.name}
              onChange={handleChange}
              placeholder="Enter club name"
              disabled={isLoading}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="city">City *</label>
            <input
              type="text"
              id="city"
              name="city"
              value={formData.city}
              onChange={handleChange}
              placeholder="Enter city name"
              disabled={isLoading}
              required
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="foundedYear">Founded Year *</label>
              <input
                type="number"
                id="foundedYear"
                name="foundedYear"
                value={formData.foundedYear}
                onChange={handleChange}
                min="1800"
                max={new Date().getFullYear()}
                disabled={isLoading}
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="budget">Budget (optional)</label>
              <input
                type="number"
                id="budget"
                name="budget"
                value={formData.budget}
                onChange={handleChange}
                placeholder="Enter budget"
                min="0"
                step="0.01"
                disabled={isLoading}
              />
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="president">President (optional)</label>
            <input
              type="text"
              id="president"
              name="president"
              value={formData.president}
              onChange={handleChange}
              placeholder="Enter president name"
              disabled={isLoading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="logoUrl">Logo URL (optional)</label>
            <input
              type="url"
              id="logoUrl"
              name="logoUrl"
              value={formData.logoUrl}
              onChange={handleChange}
              placeholder="Enter logo URL"
              disabled={isLoading}
            />
          </div>

          <div className="form-actions">
            <button type="button" className="btn btn-secondary" onClick={onClose} disabled={isLoading}>
              Cancel
            </button>
            <button type="submit" className="btn btn-primary" disabled={isLoading}>
              {isLoading ? 'Saving...' : club ? 'Update Club' : 'Create Club'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default ClubForm;
