import React, { useState, useEffect } from 'react';
import { Match, CreateMatchDto, UpdateMatchDto, Club } from '../types';

interface MatchFormProps {
  match?: Match | null;
  clubs: Club[];
  seasons: any[];
  stadiums: any[];
  isLoading: boolean;
  onSubmit: (data: CreateMatchDto | UpdateMatchDto) => Promise<void>;
  onClose: () => void;
}

const MatchForm: React.FC<MatchFormProps> = ({
  match,
  clubs,
  seasons,
  stadiums,
  isLoading,
  onSubmit,
  onClose,
}) => {
  const [formData, setFormData] = useState<any>({
    homeClubId: '',
    awayClubId: '',
    stadiumId: '',
    matchDate: '',
    time: '',
    seasonId: '',
    competitionType: '',
    homeScore: '',
    awayScore: '',
    status: 'Scheduled',
  });

  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (match) {
      setFormData({
        homeClubId: match.homeClubId,
        awayClubId: match.awayClubId,
        stadiumId: match.stadiumId,
        matchDate: match.matchDate.split('T')[0],
        time: match.time || '',
        seasonId: match.seasonId,
        competitionType: match.competitionType || '',
        homeScore: match.homeScore ?? '',
        awayScore: match.awayScore ?? '',
        status: match.status,
      });
    }
  }, [match]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData((prev: any) => ({
      ...prev,
      [name]: value,
    }));
  };

  const validateForm = (): boolean => {
    if (!formData.homeClubId || !formData.awayClubId) {
      setError('Please select both home and away clubs');
      return false;
    }

    if (formData.homeClubId === formData.awayClubId) {
      setError('Home and away clubs must be different');
      return false;
    }

    if (!formData.stadiumId) {
      setError('Please select a stadium');
      return false;
    }

    if (!formData.matchDate) {
      setError('Please select a match date');
      return false;
    }

    if (!formData.seasonId) {
      setError('Please select a season');
      return false;
    }

    if (match) {
      if (formData.homeScore === '' || formData.awayScore === '') {
        setError('Please enter both scores');
        return false;
      }
      if (formData.homeScore < 0 || formData.awayScore < 0) {
        setError('Scores cannot be negative');
        return false;
      }
    }

    return true;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!validateForm()) return;

    setIsSubmitting(true);
    try {
      if (match) {
        // Update mode
        const updateData: UpdateMatchDto = {
          homeScore: parseInt(formData.homeScore),
          awayScore: parseInt(formData.awayScore),
          status: formData.status,
        };
        await onSubmit(updateData);
      } else {
        // Create mode
        const createData: CreateMatchDto = {
          homeClubId: parseInt(formData.homeClubId),
          awayClubId: parseInt(formData.awayClubId),
          stadiumId: parseInt(formData.stadiumId),
          matchDate: formData.matchDate,
          time: formData.time || undefined,
          seasonId: parseInt(formData.seasonId),
          competitionType: formData.competitionType || undefined,
        };
        await onSubmit(createData);
      }
    } catch (err: any) {
      setError(err.message);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{match ? 'Edit Match' : 'Create New Match'}</h2>
          <button className="close-btn" onClick={onClose}>×</button>
        </div>

        {error && <div className="error-message">{error}</div>}

        <form onSubmit={handleSubmit} className="match-form">
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="homeClubId">Home Club *</label>
              <select
                id="homeClubId"
                name="homeClubId"
                value={formData.homeClubId}
                onChange={handleChange}
                disabled={match !== undefined}
                required
              >
                <option value="">Select home club</option>
                {clubs.map((club) => (
                  <option key={club.id} value={club.id}>
                    {club.name}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label htmlFor="awayClubId">Away Club *</label>
              <select
                id="awayClubId"
                name="awayClubId"
                value={formData.awayClubId}
                onChange={handleChange}
                disabled={match !== undefined}
                required
              >
                <option value="">Select away club</option>
                {clubs.map((club) => (
                  <option key={club.id} value={club.id}>
                    {club.name}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="matchDate">Match Date *</label>
              <input
                type="date"
                id="matchDate"
                name="matchDate"
                value={formData.matchDate}
                onChange={handleChange}
                disabled={match !== undefined}
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="time">Match Time</label>
              <input
                type="time"
                id="time"
                name="time"
                value={formData.time}
                onChange={handleChange}
                disabled={match !== undefined}
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="stadiumId">Stadium *</label>
              <select
                id="stadiumId"
                name="stadiumId"
                value={formData.stadiumId}
                onChange={handleChange}
                disabled={match !== undefined}
                required
              >
                <option value="">Select stadium</option>
                {stadiums.map((stadium) => (
                  <option key={stadium.id} value={stadium.id}>
                    {stadium.name}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label htmlFor="seasonId">Season *</label>
              <select
                id="seasonId"
                name="seasonId"
                value={formData.seasonId}
                onChange={handleChange}
                disabled={match !== undefined}
                required
              >
                <option value="">Select season</option>
                {seasons.map((season) => (
                  <option key={season.id} value={season.id}>
                    {season.name}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="competitionType">Competition Type</label>
              <input
                type="text"
                id="competitionType"
                name="competitionType"
                value={formData.competitionType}
                onChange={handleChange}
                placeholder="e.g., League, Cup, Friendly"
                disabled={match !== undefined}
              />
            </div>
          </div>

          {match && (
            <div className="form-row">
              <div className="form-group">
                <label htmlFor="homeScore">Home Score *</label>
                <input
                  type="number"
                  id="homeScore"
                  name="homeScore"
                  value={formData.homeScore}
                  onChange={handleChange}
                  min="0"
                  required
                />
              </div>

              <div className="form-group">
                <label htmlFor="awayScore">Away Score *</label>
                <input
                  type="number"
                  id="awayScore"
                  name="awayScore"
                  value={formData.awayScore}
                  onChange={handleChange}
                  min="0"
                  required
                />
              </div>

              <div className="form-group">
                <label htmlFor="status">Status *</label>
                <select
                  id="status"
                  name="status"
                  value={formData.status}
                  onChange={handleChange}
                  required
                >
                  <option value="Scheduled">Scheduled</option>
                  <option value="Ongoing">Ongoing</option>
                  <option value="Completed">Completed</option>
                </select>
              </div>
            </div>
          )}

          <div className="form-actions">
            <button type="button" className="btn btn-secondary" onClick={onClose} disabled={isSubmitting}>
              Cancel
            </button>
            <button type="submit" className="btn btn-primary" disabled={isSubmitting || isLoading}>
              {isSubmitting ? 'Saving...' : match ? 'Update Match' : 'Create Match'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default MatchForm;
