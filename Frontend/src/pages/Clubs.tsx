import React, { useEffect, useState, useCallback } from 'react';
import { useAuth } from '../context/AuthContext';
import { Club, ClubListResponse } from '../types';
import { clubService } from '../services/clubService';
import ClubForm from '../components/ClubForm';
import ClubList from '../components/ClubList';
import '../styles/Management.css';

export const Clubs: React.FC = () => {
  const { user } = useAuth();
  const [clubs, setClubs] = useState<Club[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editingClub, setEditingClub] = useState<Club | null>(null);

  // Pagination state
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  // Filter and search state
  const [searchTerm, setSearchTerm] = useState('');
  const [cityFilter, setCityFilter] = useState('');
  const [cities, setCities] = useState<string[]>([]);

  // Debounce timer
  const [searchTimeout, setSearchTimeout] = useState<ReturnType<typeof setTimeout> | null>(null);

  const loadClubs = useCallback(async (page: number = 1, search?: string, city?: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const response: ClubListResponse = await clubService.getClubs(page, pageSize, search, city);
      setClubs(response.data);
      setTotalPages(response.totalPages);
      setTotalCount(response.totalCount);
      setCurrentPage(page);
    } catch (err: any) {
      setError(err.message || 'Failed to load clubs');
    } finally {
      setIsLoading(false);
    }
  }, [pageSize]);

  useEffect(() => {
    loadClubs(1);
  }, [loadClubs]);

  // Load unique cities for filter
  useEffect(() => {
    const loadCities = async () => {
      try {
        const allClubs = await clubService.getAllClubs();
        const uniqueCities = [...new Set(allClubs.map(c => c.city))].sort();
        setCities(uniqueCities);
      } catch (err: any) {
        console.error('Failed to load cities:', err);
      }
    };
    loadCities();
  }, []);

  // Debounced search
  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setSearchTerm(value);

    if (searchTimeout) clearTimeout(searchTimeout);
    const timeout = setTimeout(() => {
      loadClubs(1, value, cityFilter);
    }, 500);
    setSearchTimeout(timeout);
  };

  const handleCityFilter = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const value = e.target.value;
    setCityFilter(value);
    loadClubs(1, searchTerm, value);
  };

  const handleCreateClick = () => {
    setEditingClub(null);
    setShowForm(true);
  };

  const handleEditClick = (club: Club) => {
    setEditingClub(club);
    setShowForm(true);
  };

  const handleFormClose = () => {
    setShowForm(false);
    setEditingClub(null);
  };

  const handleFormSubmit = async () => {
    const action = editingClub ? 'updated' : 'created';
    setSuccess(`Club ${action} successfully!`);
    setTimeout(() => setSuccess(null), 3000);
    await loadClubs(1, searchTerm, cityFilter);
    handleFormClose();
  };

  const handleDeleteClub = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this club?')) {
      try {
        await clubService.deleteClub(id);
        setSuccess('Club deleted successfully!');
        setTimeout(() => setSuccess(null), 3000);
        await loadClubs(currentPage, searchTerm, cityFilter);
      } catch (err: any) {
        setError(err.message || 'Failed to delete club');
      }
    }
  };

  const handlePageChange = (newPage: number) => {
    loadClubs(newPage, searchTerm, cityFilter);
  };

  // Role-based permissions
  const canCreate = user?.role === 'Admin' || user?.role === 'Manager';
  const canEdit = user?.role === 'Admin' || user?.role === 'Manager';
  const canDelete = user?.role === 'Admin';

  return (
    <div className="management-container">
      <div className="management-header">
        <h1>⚽ Clubs Management</h1>
        <div className="header-info">
          <p>Logged in as: <strong>{user?.username}</strong> ({user?.role})</p>
        </div>
      </div>

      {error && (
        <div className="alert alert-error">
          {error}
          <button className="alert-close" onClick={() => setError(null)}>✕</button>
        </div>
      )}

      {success && (
        <div className="alert alert-success">
          {success}
          <button className="alert-close" onClick={() => setSuccess(null)}>✕</button>
        </div>
      )}

      <div className="management-toolbar">
        <div className="toolbar-left">
          {canCreate && (
            <button
              className="btn btn-primary"
              onClick={handleCreateClick}
              disabled={isLoading || showForm}
            >
              + Add New Club
            </button>
          )}
        </div>

        <div className="toolbar-right">
          <div className="search-box">
            <input
              type="text"
              placeholder="Search by club name..."
              value={searchTerm}
              onChange={handleSearchChange}
              disabled={isLoading}
              className="search-input"
            />
            {searchTerm && (
              <button
                className="search-clear"
                onClick={() => {
                  setSearchTerm('');
                  loadClubs(1, '', cityFilter);
                }}
              >
                ✕
              </button>
            )}
          </div>

          <select
            className="filter-select"
            value={cityFilter}
            onChange={handleCityFilter}
            disabled={isLoading}
          >
            <option value="">All Cities</option>
            {cities.map((city) => (
              <option key={city} value={city}>
                {city}
              </option>
            ))}
          </select>
        </div>
      </div>

      {showForm && (
        <ClubForm
          club={editingClub}
          onClose={handleFormClose}
          onSubmit={handleFormSubmit}
        />
      )}

      <ClubList
        clubs={clubs}
        isLoading={isLoading}
        onEdit={handleEditClick}
        onDelete={handleDeleteClub}
        canEdit={canEdit}
        canDelete={canDelete}
      />

      {/* Pagination */}
      {!isLoading && totalPages > 1 && (
        <div className="pagination">
          <button
            className="pagination-btn"
            onClick={() => handlePageChange(currentPage - 1)}
            disabled={currentPage === 1}
          >
            ← Previous
          </button>

          <div className="pagination-info">
            Page {currentPage} of {totalPages} ({totalCount} total clubs)
          </div>

          <button
            className="pagination-btn"
            onClick={() => handlePageChange(currentPage + 1)}
            disabled={currentPage === totalPages}
          >
            Next →
          </button>
        </div>
      )}
    </div>
  );
};

export default Clubs;
