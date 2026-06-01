import React, { useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { seasonService } from '../services/seasonService';
import { Season, CreateSeasonDto, UpdateSeasonDto, Pagination } from '../types';
import '../styles/SponsorsSeasons.css';
import '../styles/Form.css';

const formatDate = (value: string) => new Date(value).toLocaleDateString();

const getErrorMessage = (error: any) => {
  if (!error) return 'An unexpected error occurred.';
  if (typeof error === 'string') return error;
  if (error.message) return error.message;
  if (error.data?.message) return error.data.message;
  return 'An unexpected error occurred.';
};

const SeasonForm: React.FC<{
  season: Season | null;
  onClose: () => void;
  onSaved: (message: string) => void;
}> = ({ season, onClose, onSaved }) => {
  const [formState, setFormState] = useState({
    name: season?.name || '',
    startDate: season?.startDate.slice(0, 10) || '',
    endDate: season?.endDate.slice(0, 10) || '',
    description: season?.description || '',
  });
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (season) {
      setFormState({
        name: season.name,
        startDate: season.startDate.slice(0, 10),
        endDate: season.endDate.slice(0, 10),
        description: season.description || '',
      });
    } else {
      setFormState({ name: '', startDate: '', endDate: '', description: '' });
    }
  }, [season]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormState((prev) => ({ ...prev, [name]: value }));
  };

  const validate = () => {
    if (!formState.name.trim()) {
      setError('Season name is required.');
      return false;
    }
    if (!formState.startDate || !formState.endDate) {
      setError('Start and end dates are required.');
      return false;
    }
    if (new Date(formState.startDate) >= new Date(formState.endDate)) {
      setError('Start date must be before end date.');
      return false;
    }
    return true;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    setError(null);
    setIsSaving(true);

    try {
      const payload: CreateSeasonDto | UpdateSeasonDto = {
        name: formState.name.trim(),
        startDate: formState.startDate,
        endDate: formState.endDate,
        description: formState.description.trim() || undefined,
      };

      if (season) {
        await seasonService.updateSeason(season.id, payload as UpdateSeasonDto);
        onSaved('Season updated successfully.');
      } else {
        await seasonService.createSeason(payload as CreateSeasonDto);
        onSaved('Season created successfully.');
      }
      onClose();
    } catch (err: any) {
      setError(getErrorMessage(err));
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="form-overlay" onClick={onClose}>
      <div className="form-container" onClick={(e) => e.stopPropagation()}>
        <div className="form-header">
          <h2>{season ? 'Edit Season' : 'Create Season'}</h2>
          <button className="close-btn" onClick={onClose} disabled={isSaving}>
            ×
          </button>
        </div>

        {error && <div className="error-message">{error}</div>}

        <form onSubmit={handleSubmit} className="player-form">
          <div className="form-group">
            <label htmlFor="season-name">Season Name *</label>
            <input
              id="season-name"
              name="name"
              type="text"
              value={formState.name}
              onChange={handleChange}
              placeholder="2026/2027 Season"
              disabled={isSaving}
              required
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="season-start">Start Date *</label>
              <input
                id="season-start"
                name="startDate"
                type="date"
                value={formState.startDate}
                onChange={handleChange}
                disabled={isSaving}
                required
              />
            </div>
            <div className="form-group">
              <label htmlFor="season-end">End Date *</label>
              <input
                id="season-end"
                name="endDate"
                type="date"
                value={formState.endDate}
                onChange={handleChange}
                disabled={isSaving}
                required
              />
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="season-description">Description</label>
            <textarea
              id="season-description"
              name="description"
              value={formState.description}
              onChange={handleChange}
              placeholder="Optional season notes"
              disabled={isSaving}
              rows={4}
            />
          </div>

          <div className="form-actions">
            <button type="button" className="btn btn-secondary" onClick={onClose} disabled={isSaving}>
              Cancel
            </button>
            <button type="submit" className="btn btn-success" disabled={isSaving}>
              {isSaving ? 'Saving...' : season ? 'Save Season' : 'Create Season'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

const Seasons: React.FC = () => {
  const { user } = useAuth();
  const [seasons, setSeasons] = useState<Season[]>([]);
  const [pagination, setPagination] = useState<Pagination>({
    page: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0,
  });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editingSeason, setEditingSeason] = useState<Season | null>(null);

  const canManage = user?.role === 'Admin';

  const loadSeasons = async (page = 1) => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await seasonService.getSeasons(page, pagination.pageSize);
      setSeasons(result.data);
      setPagination(result.pagination);
    } catch (err: any) {
      setError(getErrorMessage(err));
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadSeasons(pagination.page);
  }, [pagination.page, pagination.pageSize]);

  const openCreateForm = () => {
    if (!canManage) {
      setError('Only administrators can manage seasons.');
      return;
    }
    setEditingSeason(null);
    setShowForm(true);
  };

  const openEditForm = (season: Season) => {
    if (!canManage) {
      setError('Only administrators can manage seasons.');
      return;
    }
    setEditingSeason(season);
    setShowForm(true);
  };

  const closeForm = () => {
    setShowForm(false);
    setEditingSeason(null);
  };

  const handleSaveSuccess = (message: string) => {
    setSuccessMessage(message);
    loadSeasons(pagination.page);
    setTimeout(() => setSuccessMessage(null), 3000);
  };

  const handleDeleteSeason = async (season: Season) => {
    if (!canManage) {
      setError('Only administrators can delete seasons.');
      return;
    }
    if (!window.confirm(`Delete season ${season.name}? This cannot be undone.`)) {
      return;
    }
    setError(null);
    try {
      await seasonService.deleteSeason(season.id);
      setSuccessMessage('Season deleted successfully.');
      loadSeasons(pagination.page);
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(getErrorMessage(err));
    }
  };

  const handlePageChange = (page: number) => {
    setPagination((prev) => ({ ...prev, page }));
  };

  return (
    <div className="sponsors-seasons-page">
      <div className="page-header">
        <div>
          <h1>Season Management</h1>
          <p>Track annual seasons, date ranges, and season notes for club operations.</p>
        </div>
        <div className="header-actions">
          {canManage && (
            <button className="btn btn-success" onClick={openCreateForm}>
              + Create Season
            </button>
          )}
        </div>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {successMessage && <div className="alert alert-success">{successMessage}</div>}

      <div className="tab-panel">
        <div className="table-wrapper">
          <table className="data-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Start Date</th>
                <th>End Date</th>
                <th>Description</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {seasons.map((season) => (
                <tr key={season.id}>
                  <td>{season.name}</td>
                  <td>{formatDate(season.startDate)}</td>
                  <td>{formatDate(season.endDate)}</td>
                  <td>{season.description || '-'}</td>
                  <td className="actions-cell">
                    <button className="btn btn-success" onClick={() => openEditForm(season)}>
                      Edit
                    </button>
                    {canManage && (
                      <button className="btn btn-delete" onClick={() => handleDeleteSeason(season)}>
                        Delete
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {isLoading && <p>Loading seasons…</p>}
          {!isLoading && seasons.length === 0 && <p>No seasons available.</p>}
        </div>

        <div className="pagination-bar">
          <div>
            Page {pagination.page} of {pagination.totalPages}
          </div>
          <div className="actions-cell">
            <button className="btn btn-sm" onClick={() => handlePageChange(Math.max(1, pagination.page - 1))} disabled={pagination.page <= 1}>
              Previous
            </button>
            <button className="btn btn-sm" onClick={() => handlePageChange(Math.min(pagination.totalPages, pagination.page + 1))} disabled={pagination.page >= pagination.totalPages}>
              Next
            </button>
          </div>
        </div>
      </div>

      {showForm && <SeasonForm season={editingSeason} onClose={closeForm} onSaved={handleSaveSuccess} />}
    </div>
  );
};

export default Seasons;
