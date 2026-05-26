import React, { useEffect, useMemo, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { sponsorService } from '../services/sponsorService';
import { seasonService } from '../services/seasonService';
import {
  Sponsor,
  SponsorDetail,
  CreateSponsorDto,
  UpdateSponsorDto,
  Season,
  CreateSeasonDto,
  UpdateSeasonDto,
  Pagination,
} from '../types';
import '../styles/SponsorsSeasons.css';

type Tab = 'sponsors' | 'seasons';

type DeleteTarget = {
  type: 'sponsor' | 'season';
  id: number;
  name: string;
};

const formatDate = (value: string) => new Date(value).toLocaleDateString();

const getErrorMessage = (error: any) => {
  if (!error) return 'An unexpected error occurred.';
  if (typeof error === 'string') return error;
  if (error.message) return error.message;
  if (error.data?.message) return error.data.message;
  return 'An unexpected error occurred.';
};

const SponsorForm: React.FC<{
  sponsor: Sponsor | null;
  onClose: () => void;
  onSaved: (message: string) => void;
}> = ({ sponsor, onClose, onSaved }) => {
  const [formState, setFormState] = useState({
    name: sponsor?.name || '',
    logo: sponsor?.logo || '',
    website: sponsor?.website || '',
  });
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (sponsor) {
      setFormState({
        name: sponsor.name,
        logo: sponsor.logo || '',
        website: sponsor.website || '',
      });
    } else {
      setFormState({ name: '', logo: '', website: '' });
    }
  }, [sponsor]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormState((prev) => ({ ...prev, [name]: value }));
  };

  const validate = () => {
    if (!formState.name.trim()) {
      setError('Sponsor name is required.');
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
      const payload: CreateSponsorDto | UpdateSponsorDto = {
        name: formState.name.trim(),
        logo: formState.logo.trim() || undefined,
        website: formState.website.trim() || undefined,
      };

      if (sponsor) {
        await sponsorService.updateSponsor(sponsor.id, payload as UpdateSponsorDto);
        onSaved('Sponsor updated successfully.');
      } else {
        await sponsorService.createSponsor(payload as CreateSponsorDto);
        onSaved('Sponsor created successfully.');
      }
      onClose();
    } catch (err: any) {
      setError(getErrorMessage(err));
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="form-overlay">
      <div className="form-container">
        <div className="form-header">
          <h2>{sponsor ? 'Edit Sponsor' : 'Create Sponsor'}</h2>
          <button className="close-btn" onClick={onClose} disabled={isSaving}>
            ×
          </button>
        </div>

        {error && <div className="error-message">{error}</div>}

        <form onSubmit={handleSubmit} className="player-form">
          <div className="form-group">
            <label htmlFor="sponsor-name">Name *</label>
            <input
              id="sponsor-name"
              name="name"
              type="text"
              value={formState.name}
              onChange={handleChange}
              placeholder="Sponsor name"
              disabled={isSaving}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="sponsor-logo">Logo URL</label>
            <input
              id="sponsor-logo"
              name="logo"
              type="text"
              value={formState.logo}
              onChange={handleChange}
              placeholder="https://example.com/logo.png"
              disabled={isSaving}
            />
          </div>

          <div className="form-group">
            <label htmlFor="sponsor-website">Website URL</label>
            <input
              id="sponsor-website"
              name="website"
              type="text"
              value={formState.website}
              onChange={handleChange}
              placeholder="https://example.com"
              disabled={isSaving}
            />
          </div>

          <div className="form-actions">
            <button type="button" className="btn btn-secondary" onClick={onClose} disabled={isSaving}>
              Cancel
            </button>
            <button type="submit" className="btn btn-success" disabled={isSaving}>
              {isSaving ? 'Saving...' : sponsor ? 'Save Sponsor' : 'Create Sponsor'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
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
    <div className="form-overlay">
      <div className="form-container">
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
              rows={3}
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

const DeleteConfirmationModal: React.FC<{
  target: DeleteTarget;
  onCancel: () => void;
  onConfirm: () => void;
  isDeleting: boolean;
}> = ({ target, onCancel, onConfirm, isDeleting }) => (
  <div className="form-overlay">
    <div className="form-container">
      <div className="form-header">
        <h2>Confirm Delete</h2>
      </div>
      <div className="form-body">
        <p>
          Are you sure you want to delete the {target.type === 'sponsor' ? 'sponsor' : 'season'}{' '}
          <strong>{target.name}</strong>?
        </p>
        <p>This action cannot be undone.</p>
      </div>
      <div className="form-actions">
        <button type="button" className="btn btn-secondary" onClick={onCancel} disabled={isDeleting}>
          Cancel
        </button>
        <button type="button" className="btn btn-delete" onClick={onConfirm} disabled={isDeleting}>
          {isDeleting ? 'Deleting...' : 'Delete'}
        </button>
      </div>
    </div>
  </div>
);

const SponsorsSeasons: React.FC = () => {
  const { isAdmin } = useAuth();
  const [activeTab, setActiveTab] = useState<Tab>('sponsors');

  const [sponsors, setSponsors] = useState<Sponsor[]>([]);
  const [sponsorPagination, setSponsorPagination] = useState<Pagination>({ page: 1, pageSize: 10, totalCount: 0, totalPages: 1 });
  const [expandedSponsorIds, setExpandedSponsorIds] = useState<number[]>([]);
  const [sponsorDetails, setSponsorDetails] = useState<Record<number, SponsorDetail>>({});
  const [sponsorDetailsLoadingId, setSponsorDetailsLoadingId] = useState<number | null>(null);
  const [isLoadingSponsors, setIsLoadingSponsors] = useState(false);
  const [sponsorError, setSponsorError] = useState<string | null>(null);
  const [sponsorSuccess, setSponsorSuccess] = useState<string | null>(null);
  const [showSponsorForm, setShowSponsorForm] = useState(false);
  const [editingSponsor, setEditingSponsor] = useState<Sponsor | null>(null);

  const [seasons, setSeasons] = useState<Season[]>([]);
  const [seasonPagination, setSeasonPagination] = useState<Pagination>({ page: 1, pageSize: 10, totalCount: 0, totalPages: 1 });
  const [isLoadingSeasons, setIsLoadingSeasons] = useState(false);
  const [seasonError, setSeasonError] = useState<string | null>(null);
  const [seasonSuccess, setSeasonSuccess] = useState<string | null>(null);
  const [showSeasonForm, setShowSeasonForm] = useState(false);
  const [editingSeason, setEditingSeason] = useState<Season | null>(null);

  const [deleteTarget, setDeleteTarget] = useState<DeleteTarget | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  const currentDate = useMemo(() => new Date(), []);

  useEffect(() => {
    loadSponsors(1);
    loadSeasons(1);
  }, []);

  useEffect(() => {
    if (sponsorSuccess) {
      const timeout = window.setTimeout(() => setSponsorSuccess(null), 4000);
      return () => window.clearTimeout(timeout);
    }
  }, [sponsorSuccess]);

  useEffect(() => {
    if (seasonSuccess) {
      const timeout = window.setTimeout(() => setSeasonSuccess(null), 4000);
      return () => window.clearTimeout(timeout);
    }
  }, [seasonSuccess]);

  const loadSponsors = async (page = 1) => {
    setIsLoadingSponsors(true);
    setSponsorError(null);

    try {
      const result = await sponsorService.getSponsors(page, sponsorPagination.pageSize);
      setSponsors(result.data);
      setSponsorPagination(result.pagination);
    } catch (error: any) {
      setSponsorError(getErrorMessage(error));
    } finally {
      setIsLoadingSponsors(false);
    }
  };

  const loadSeasons = async (page = 1) => {
    setIsLoadingSeasons(true);
    setSeasonError(null);

    try {
      const result = await seasonService.getSeasons(page, seasonPagination.pageSize);
      setSeasons(result.data);
      setSeasonPagination(result.pagination);
    } catch (error: any) {
      setSeasonError(getErrorMessage(error));
    } finally {
      setIsLoadingSeasons(false);
    }
  };

  const toggleSponsorDetails = async (sponsorId: number) => {
    if (expandedSponsorIds.includes(sponsorId)) {
      setExpandedSponsorIds((prev) => prev.filter((id) => id !== sponsorId));
      return;
    }

    setExpandedSponsorIds((prev) => [...prev, sponsorId]);

    if (!sponsorDetails[sponsorId]) {
      setSponsorDetailsLoadingId(sponsorId);
      try {
        const details = await sponsorService.getSponsorById(sponsorId);
        if (details) {
          setSponsorDetails((prev) => ({ ...prev, [sponsorId]: details }));
        }
      } catch (error: any) {
        setSponsorError(getErrorMessage(error));
      } finally {
        setSponsorDetailsLoadingId(null);
      }
    }
  };

  const handleSponsorSubmitSuccess = (message: string) => {
    setSponsorSuccess(message);
    loadSponsors(1);
  };

  const handleSeasonSubmitSuccess = (message: string) => {
    setSeasonSuccess(message);
    loadSeasons(1);
  };

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    setIsDeleting(true);

    try {
      if (deleteTarget.type === 'sponsor') {
        await sponsorService.deleteSponsor(deleteTarget.id);
        setSponsorSuccess('Sponsor deleted successfully.');
        loadSponsors(sponsorPagination.page);
      } else {
        await seasonService.deleteSeason(deleteTarget.id);
        setSeasonSuccess('Season deleted successfully.');
        loadSeasons(seasonPagination.page);
      }
    } catch (error: any) {
      if (deleteTarget.type === 'sponsor') {
        setSponsorError(getErrorMessage(error));
      } else {
        setSeasonError(getErrorMessage(error));
      }
    } finally {
      setDeleteTarget(null);
      setIsDeleting(false);
    }
  };

  const activeSeasonCount = useMemo(() => {
    return seasons.filter((season) => {
      const start = new Date(season.startDate);
      const end = new Date(season.endDate);
      return currentDate >= start && currentDate <= end;
    }).length;
  }, [seasons, currentDate]);

  return (
    <div className="sponsors-seasons-page">
      <div className="page-header">
        <div>
          <h1>Sponsors & Seasons</h1>
          <p>Manage sponsor records, season schedules, and admin workflows in one tabbed view.</p>
        </div>
        {isAdmin && (
          <div className="header-actions">
            <button className="btn btn-success" onClick={() => { setActiveTab('sponsors'); setEditingSponsor(null); setShowSponsorForm(true); }}>
              + Create Sponsor
            </button>
            <button className="btn btn-success" onClick={() => { setActiveTab('seasons'); setEditingSeason(null); setShowSeasonForm(true); }}>
              + Create Season
            </button>
          </div>
        )}
      </div>

      <div className="tabs">
        <button
          type="button"
          className={`tab-button ${activeTab === 'sponsors' ? 'active' : ''}`}
          onClick={() => setActiveTab('sponsors')}
        >
          Sponsors
        </button>
        <button
          type="button"
          className={`tab-button ${activeTab === 'seasons' ? 'active' : ''}`}
          onClick={() => setActiveTab('seasons')}
        >
          Seasons
        </button>
      </div>

      {activeTab === 'sponsors' && (
        <section className="tab-panel">
          {sponsorError && <div className="error-message">{sponsorError}</div>}
          {sponsorSuccess && <div className="success-message">{sponsorSuccess}</div>}

          {isLoadingSponsors ? (
            <div className="loading">Loading sponsors...</div>
          ) : sponsors.length === 0 ? (
            <div className="empty-state">
              <p>No sponsors found yet.</p>
              {isAdmin ? <p>Use the Create Sponsor button to add the first sponsor.</p> : <p>Contact an administrator to add sponsors.</p>}
            </div>
          ) : (
            <div className="table-wrapper">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Logo</th>
                    <th>Website</th>
                    <th>Sponsored Clubs</th>
                    <th>Created</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {sponsors.map((sponsor) => {
                    const isExpanded = expandedSponsorIds.includes(sponsor.id);
                    const details = sponsorDetails[sponsor.id];
                    return (
                      <React.Fragment key={sponsor.id}>
                        <tr>
                          <td>{sponsor.name}</td>
                          <td>
                            {sponsor.logo ? (
                              <img src={sponsor.logo} alt={`${sponsor.name} logo`} className="sponsor-logo" />
                            ) : (
                              '—'
                            )}
                          </td>
                          <td>
                            {sponsor.website ? (
                              <a href={sponsor.website} target="_blank" rel="noreferrer">
                                Visit
                              </a>
                            ) : (
                              '—'
                            )}
                          </td>
                          <td>
                            <button
                              className="btn btn-secondary btn-sm"
                              onClick={() => toggleSponsorDetails(sponsor.id)}
                              disabled={sponsorDetailsLoadingId === sponsor.id}
                            >
                              {isExpanded ? 'Hide clubs' : 'Show clubs'}
                            </button>
                          </td>
                          <td>{formatDate(sponsor.createdAt)}</td>
                          <td className="actions-cell">
                            {isAdmin && (
                              <>
                                <button
                                  className="btn btn-edit btn-sm"
                                  onClick={() => {
                                    setEditingSponsor(sponsor);
                                    setShowSponsorForm(true);
                                  }}
                                >
                                  Edit
                                </button>
                                <button
                                  className="btn btn-delete btn-sm"
                                  onClick={() => setDeleteTarget({ type: 'sponsor', id: sponsor.id, name: sponsor.name })}
                                >
                                  Delete
                                </button>
                              </>
                            )}
                          </td>
                        </tr>
                        {isExpanded && (
                          <tr className="details-row">
                            <td colSpan={6}>
                              {sponsorDetailsLoadingId === sponsor.id ? (
                                <div className="loading">Loading sponsored clubs...</div>
                              ) : (
                                <div className="details-card">
                                  <h3>Sponsored Clubs</h3>
                                  {details?.clubs.length ? (
                                    <ul>
                                      {details.clubs.map((club) => (
                                        <li key={club.id}>{club.name}</li>
                                      ))}
                                    </ul>
                                  ) : (
                                    <p>No sponsored clubs are associated with this sponsor.</p>
                                  )}
                                </div>
                              )}
                            </td>
                          </tr>
                        )}
                      </React.Fragment>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}

          {sponsors.length > 0 && (
            <div className="pagination-bar">
              <button
                className="btn btn-secondary"
                onClick={() => loadSponsors(Math.max(1, sponsorPagination.page - 1))}
                disabled={sponsorPagination.page <= 1}
              >
                Previous
              </button>
              <span>
                Page {sponsorPagination.page} of {sponsorPagination.totalPages}
                {' • '}Total {sponsorPagination.totalCount}
              </span>
              <button
                className="btn btn-secondary"
                onClick={() => loadSponsors(Math.min(sponsorPagination.totalPages, sponsorPagination.page + 1))}
                disabled={sponsorPagination.page >= sponsorPagination.totalPages}
              >
                Next
              </button>
            </div>
          )}
        </section>
      )}

      {activeTab === 'seasons' && (
        <section className="tab-panel">
          {seasonError && <div className="error-message">{seasonError}</div>}
          {seasonSuccess && <div className="success-message">{seasonSuccess}</div>}

          <div className="season-summary">
            <p>Current active season count: <strong>{activeSeasonCount}</strong></p>
          </div>

          {isLoadingSeasons ? (
            <div className="loading">Loading seasons...</div>
          ) : seasons.length === 0 ? (
            <div className="empty-state">
              <p>No seasons found.</p>
              {isAdmin ? <p>Use the Create Season button to add the first season.</p> : <p>Contact an administrator to add seasons.</p>}
            </div>
          ) : (
            <div className="table-wrapper">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Date Range</th>
                    <th>Status</th>
                    <th>Description</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {seasons.map((season) => {
                    const start = new Date(season.startDate);
                    const end = new Date(season.endDate);
                    const isCurrent = currentDate >= start && currentDate <= end;
                    return (
                      <tr key={season.id}>
                        <td>{season.name}</td>
                        <td>{`${formatDate(season.startDate)} — ${formatDate(season.endDate)}`}</td>
                        <td>
                          {isCurrent ? <span className="badge badge-current">Current</span> : <span className="badge badge-muted">Planned</span>}
                        </td>
                        <td>{season.description || '—'}</td>
                        <td className="actions-cell">
                          {isAdmin && (
                            <>
                              <button
                                className="btn btn-edit btn-sm"
                                onClick={() => {
                                  setEditingSeason(season);
                                  setShowSeasonForm(true);
                                }}
                              >
                                Edit
                              </button>
                              <button
                                className="btn btn-delete btn-sm"
                                onClick={() => setDeleteTarget({ type: 'season', id: season.id, name: season.name })}
                              >
                                Delete
                              </button>
                            </>
                          )}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}

          {seasons.length > 0 && (
            <div className="pagination-bar">
              <button
                className="btn btn-secondary"
                onClick={() => loadSeasons(Math.max(1, seasonPagination.page - 1))}
                disabled={seasonPagination.page <= 1}
              >
                Previous
              </button>
              <span>
                Page {seasonPagination.page} of {seasonPagination.totalPages}
                {' • '}Total {seasonPagination.totalCount}
              </span>
              <button
                className="btn btn-secondary"
                onClick={() => loadSeasons(Math.min(seasonPagination.totalPages, seasonPagination.page + 1))}
                disabled={seasonPagination.page >= seasonPagination.totalPages}
              >
                Next
              </button>
            </div>
          )}
        </section>
      )}

      {showSponsorForm && (
        <SponsorForm
          sponsor={editingSponsor}
          onClose={() => {
            setShowSponsorForm(false);
            setEditingSponsor(null);
          }}
          onSaved={handleSponsorSubmitSuccess}
        />
      )}

      {showSeasonForm && (
        <SeasonForm
          season={editingSeason}
          onClose={() => {
            setShowSeasonForm(false);
            setEditingSeason(null);
          }}
          onSaved={handleSeasonSubmitSuccess}
        />
      )}

      {deleteTarget && (
        <DeleteConfirmationModal
          target={deleteTarget}
          onCancel={() => setDeleteTarget(null)}
          onConfirm={confirmDelete}
          isDeleting={isDeleting}
        />
      )}
    </div>
  );
};

export default SponsorsSeasons;
