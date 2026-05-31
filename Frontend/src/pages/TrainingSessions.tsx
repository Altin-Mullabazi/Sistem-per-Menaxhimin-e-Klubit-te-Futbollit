import React, { useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { trainingService } from '../services/trainingService';
import { clubService } from '../services/clubService';
import { TrainingSession, Club, CreateTrainingSessionDto, UpdateTrainingSessionDto, Pagination, TrainingType } from '../types';
import '../styles/Matches.css';
import '../styles/Form.css';

const trainingTypeLabels: Record<TrainingType, string> = {
  1: 'Tactical',
  2: 'Physical',
  3: 'Friendly',
  4: 'Recovery',
  5: 'Set Pieces',
};

const getErrorMessage = (error: any) => {
  if (!error) return 'An unexpected error occurred.';
  if (typeof error === 'string') return error;
  if (error.message) return error.message;
  if (error.data?.message) return error.data.message;
  return 'An unexpected error occurred.';
};

const TrainingForm: React.FC<{
  session: TrainingSession | null;
  clubs: Club[];
  onClose: () => void;
  onSubmit: (payload: CreateTrainingSessionDto | UpdateTrainingSessionDto) => Promise<void>;
}> = ({ session, clubs, onClose, onSubmit }) => {
  const [formState, setFormState] = useState({
    clubId: session?.clubId ?? '',
    sessionDate: session?.sessionDate.slice(0, 10) ?? '',
    duration: session?.duration ?? 60,
    type: session?.type ?? 1,
    notes: session?.notes ?? '',
  });
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    setFormState({
      clubId: session?.clubId ?? '',
      sessionDate: session?.sessionDate.slice(0, 10) ?? '',
      duration: session?.duration ?? 60,
      type: session?.type ?? 1,
      notes: session?.notes ?? '',
    });
  }, [session]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormState((prev) => ({
      ...prev,
      [name]: name === 'duration' || name === 'clubId' || name === 'type' ? Number(value) : value,
    }));
  };

  const validate = () => {
    if (!formState.clubId) {
      setError('Please choose a club.');
      return false;
    }
    if (!formState.sessionDate) {
      setError('Please provide a training date.');
      return false;
    }
    if (formState.duration < 15 || formState.duration > 180) {
      setError('Duration must be between 15 and 180 minutes.');
      return false;
    }
    return true;
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setError(null);
    if (!validate()) return;

    setIsSubmitting(true);
    try {
      if (session) {
        const payload: UpdateTrainingSessionDto = {
          sessionDate: formState.sessionDate,
          duration: formState.duration,
          type: formState.type,
          notes: formState.notes.trim() || undefined,
        };
        await onSubmit(payload);
      } else {
        const payload: CreateTrainingSessionDto = {
          clubId: Number(formState.clubId),
          sessionDate: formState.sessionDate,
          duration: formState.duration,
          type: formState.type,
          notes: formState.notes.trim() || undefined,
        };
        await onSubmit(payload);
      }
    } catch (err: any) {
      setError(getErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{session ? 'Edit Training Session' : 'Create Training Session'}</h2>
          <button className="close-btn" onClick={onClose}>
            ×
          </button>
        </div>

        {error && <div className="error-message">{error}</div>}

        <form className="match-form" onSubmit={handleSubmit}>
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="clubId">Club *</label>
              <select
                id="clubId"
                name="clubId"
                value={formState.clubId}
                onChange={handleChange}
                required
              >
                <option value="">Select a club</option>
                {clubs.map((club) => (
                  <option key={club.id} value={club.id}>
                    {club.name}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label htmlFor="sessionDate">Date *</label>
              <input
                id="sessionDate"
                name="sessionDate"
                type="date"
                value={formState.sessionDate}
                onChange={handleChange}
                required
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="duration">Duration (minutes) *</label>
              <input
                id="duration"
                name="duration"
                type="number"
                value={formState.duration}
                onChange={handleChange}
                min={15}
                max={180}
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="type">Type *</label>
              <select id="type" name="type" value={formState.type} onChange={handleChange} required>
                {Object.entries(trainingTypeLabels).map(([value, label]) => (
                  <option key={value} value={value}>
                    {label}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="notes">Notes</label>
            <textarea
              id="notes"
              name="notes"
              value={formState.notes}
              onChange={handleChange}
              rows={4}
              placeholder="Optional notes for the training session"
            />
          </div>

          <div className="form-actions">
            <button type="button" className="btn btn-secondary" onClick={onClose} disabled={isSubmitting}>
              Cancel
            </button>
            <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
              {isSubmitting ? 'Saving...' : session ? 'Save Session' : 'Create Session'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

const TrainingSessions: React.FC = () => {
  const { user } = useAuth();
  const [sessions, setSessions] = useState<TrainingSession[]>([]);
  const [clubs, setClubs] = useState<Club[]>([]);
  const [pagination, setPagination] = useState<Pagination>({
    page: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0,
  });
  const [filterClubId, setFilterClubId] = useState<number | undefined>();
  const [startDate, setStartDate] = useState<string>('');
  const [endDate, setEndDate] = useState<string>('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editingSession, setEditingSession] = useState<TrainingSession | null>(null);

  const canManage = user?.role === 'Admin' || user?.role === 'Manager';
  const canDelete = user?.role === 'Admin';

  const loadClubs = async () => {
    try {
      const data = await clubService.getClubs(1, 100);
      setClubs(data);
    } catch (err: any) {
      console.warn('Unable to load clubs for training filter', err);
    }
  };

  const loadSessions = async (page = 1) => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await trainingService.getTrainingSessions(
        page,
        pagination.pageSize,
        filterClubId,
        startDate || undefined,
        endDate || undefined
      );
      setSessions(result.sessions);
      setPagination(result.pagination);
    } catch (err: any) {
      setError(getErrorMessage(err));
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadClubs();
  }, []);

  useEffect(() => {
    loadSessions(pagination.page);
  }, [pagination.page, pagination.pageSize, filterClubId, startDate, endDate]);

  const openCreateForm = () => {
    if (!canManage) {
      setError('Only managers and administrators can manage training sessions.');
      return;
    }
    setEditingSession(null);
    setShowForm(true);
  };

  const openEditForm = (session: TrainingSession) => {
    if (!canManage) {
      setError('Only managers and administrators can manage training sessions.');
      return;
    }
    setEditingSession(session);
    setShowForm(true);
  };

  const closeForm = () => {
    setShowForm(false);
    setEditingSession(null);
  };

  const handleSave = async (payload: CreateTrainingSessionDto | UpdateTrainingSessionDto) => {
    setError(null);
    try {
      if (editingSession) {
        await trainingService.updateTrainingSession(editingSession.id, payload as UpdateTrainingSessionDto);
        setSuccessMessage('Training session updated successfully.');
      } else {
        await trainingService.createTrainingSession(payload as CreateTrainingSessionDto);
        setSuccessMessage('Training session created successfully.');
      }
      closeForm();
      loadSessions(pagination.page);
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(getErrorMessage(err));
    }
  };

  const handleDeleteSession = async (id: number) => {
    if (!canDelete) {
      setError('Only administrators can delete training sessions.');
      return;
    }
    if (!window.confirm('Delete this training session?')) {
      return;
    }
    setError(null);
    try {
      await trainingService.deleteTrainingSession(id);
      setSuccessMessage('Training session deleted successfully.');
      loadSessions(pagination.page);
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err: any) {
      setError(getErrorMessage(err));
    }
  };

  return (
    <div className="matches-container">
      <div className="matches-header">
        <div>
          <h1>Training Sessions</h1>
          <p>Schedule and manage club training sessions by date, type, and club.</p>
        </div>
        <div className="header-info">
          <p>Logged in as: <strong>{user?.username || 'User'}</strong></p>
          {user?.role && <p>Role: <strong>{user.role}</strong></p>}
        </div>
      </div>

      {error && (
        <div className="alert alert-error">
          {error}
          <button className="close-btn" onClick={() => setError(null)}>×</button>
        </div>
      )}
      {successMessage && (
        <div className="alert alert-success">
          {successMessage}
          <button className="close-btn" onClick={() => setSuccessMessage(null)}>×</button>
        </div>
      )}

      <div className="matches-controls">
        {canManage && (
          <button className="btn btn-primary btn-lg" onClick={openCreateForm}>
            + New Training Session
          </button>
        )}
      </div>

      <div className="filters-section">
        <div className="filters-grid">
          <div className="filter-group">
            <label htmlFor="club-filter">Filter by Club</label>
            <select
              id="club-filter"
              value={filterClubId || ''}
              onChange={(e) => {
                setFilterClubId(e.target.value ? Number(e.target.value) : undefined);
                setPagination((prev) => ({ ...prev, page: 1 }));
              }}
            >
              <option value="">All Clubs</option>
              {clubs.map((club) => (
                <option key={club.id} value={club.id}>
                  {club.name}
                </option>
              ))}
            </select>
          </div>

          <div className="filter-group">
            <label htmlFor="start-date-filter">Start Date</label>
            <input
              id="start-date-filter"
              type="date"
              value={startDate}
              onChange={(e) => {
                setStartDate(e.target.value);
                setPagination((prev) => ({ ...prev, page: 1 }));
              }}
            />
          </div>

          <div className="filter-group">
            <label htmlFor="end-date-filter">End Date</label>
            <input
              id="end-date-filter"
              type="date"
              value={endDate}
              onChange={(e) => {
                setEndDate(e.target.value);
                setPagination((prev) => ({ ...prev, page: 1 }));
              }}
            />
          </div>
        </div>

        <div className="pagination-section">
          <div className="pagination-controls">
            <button className="btn btn-sm" onClick={() => setPagination((prev) => ({ ...prev, page: Math.max(1, prev.page - 1) }))} disabled={pagination.page === 1}>
              Previous
            </button>
            <div className="page-info">
              Page <input
                type="number"
                min="1"
                max={pagination.totalPages}
                value={pagination.page}
                onChange={(e) => {
                  const nextPage = Math.max(1, Math.min(pagination.totalPages, Number(e.target.value) || 1));
                  setPagination((prev) => ({ ...prev, page: nextPage }));
                }}
                className="page-input"
              /> of {pagination.totalPages}
            </div>
            <button className="btn btn-sm" onClick={() => setPagination((prev) => ({ ...prev, page: Math.min(pagination.totalPages, prev.page + 1) }))} disabled={pagination.page >= pagination.totalPages}>
              Next
            </button>
            <select
              value={pagination.pageSize}
              onChange={(e) => {
                setPagination((prev) => ({ ...prev, pageSize: Number(e.target.value), page: 1 }));
              }}
              className="page-size-select"
            >
              <option value="5">5 per page</option>
              <option value="10">10 per page</option>
              <option value="20">20 per page</option>
            </select>
          </div>
        </div>
      </div>

      <div className="table-container">
        <table className="matches-table">
          <thead>
            <tr>
              <th>Date</th>
              <th>Club</th>
              <th>Type</th>
              <th>Duration</th>
              <th>Notes</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {sessions.map((session) => (
              <tr key={session.id}>
                <td>{new Date(session.sessionDate).toLocaleDateString()}</td>
                <td>{session.clubName}</td>
                <td>{trainingTypeLabels[session.type]}</td>
                <td>{session.duration} min</td>
                <td>{session.notes || '-'}</td>
                <td className="actions-cell">
                  {canManage && (
                    <button className="btn btn-edit" onClick={() => openEditForm(session)}>
                      Edit
                    </button>
                  )}
                  {canDelete && (
                    <button className="btn btn-delete" onClick={() => handleDeleteSession(session.id)}>
                      Delete
                    </button>
                  )}
                </td>
              </tr>
            ))}
            {sessions.length === 0 && !isLoading && (
              <tr>
                <td colSpan={6} className="no-data">
                  No training sessions found.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {showForm && (
        <TrainingForm session={editingSession} clubs={clubs} onClose={closeForm} onSubmit={handleSave} />
      )}
    </div>
  );
};

export default TrainingSessions;
