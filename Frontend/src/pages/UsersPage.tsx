import React, { useEffect, useState, useMemo } from 'react';
import { useAuth } from '../context/AuthContext';
import UsersList from '../components/UsersList';
import UserForm from '../components/UserForm';
import DeleteConfirmation from '../components/DeleteConfirmation';
import { usersService, GetUsersResponse } from '../services/usersService';
import { User } from '../types';
import '../styles/UsersPage.css';

const debounce = (fn: (...args: any[]) => void, ms = 300) => {
  let t: any;
  return (...args: any[]) => {
    clearTimeout(t);
    t = setTimeout(() => fn(...args), ms);
  };
};

const UsersPage: React.FC = () => {
  const { isAdmin } = useAuth();
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const [search, setSearch] = useState('');
  const [searchTerm, setSearchTerm] = useState('');

  const [showCreate, setShowCreate] = useState(false);
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [deletingUser, setDeletingUser] = useState<User | null>(null);
  const [actionLoading, setActionLoading] = useState(false);

  const loadUsers = async (p = page, ps = pageSize, s = searchTerm) => {
    setLoading(true);
    setError(null);
    try {
      const res = await usersService.getUsers(p, ps, s) as GetUsersResponse;
      setUsers(res.data);
      setPage(res.pagination.currentPage);
      setPageSize(res.pagination.pageSize);
      setTotalPages(res.pagination.totalPages);
      setTotalCount(res.pagination.totalCount);
    } catch (err: any) {
      setError(err?.message || 'Error loading users');
    } finally {
      setLoading(false);
    }
  };

  // eslint-disable-next-line react-hooks/exhaustive-deps
  const debouncedSearch = useMemo(() => debounce((val: string) => { setSearchTerm(val); setPage(1); }, 300), []);

  useEffect(() => { void loadUsers(page, pageSize, searchTerm); }, [page, pageSize, searchTerm]);

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearch(e.target.value);
    debouncedSearch(e.target.value);
  };

  const handleClearSearch = () => {
    setSearch('');
    setSearchTerm('');
    setPage(1);
  };

  const handleCreate = async (data: any) => {
    setActionLoading(true);
    try {
      await usersService.createUser({ email: data.email, password: data.password, firstName: data.firstName, lastName: data.lastName, role: data.role });
      setShowCreate(false);
      await loadUsers(1, pageSize, searchTerm);
      alert('User created successfully');
    } catch (err: any) {
      throw err?.response?.data || err;
    } finally { setActionLoading(false); }
  };

  const handleEdit = async (data: any) => {
    if (!editingUser) return;
    setActionLoading(true);
    try {
      await usersService.updateUser(editingUser.id, { email: data.email, firstName: data.firstName, lastName: data.lastName });
      setEditingUser(null);
      await loadUsers(page, pageSize, searchTerm);
      alert('User updated successfully');
    } catch (err: any) {
      throw err?.response?.data || err;
    } finally { setActionLoading(false); }
  };

  const handleDelete = async () => {
    if (!deletingUser) return;
    setActionLoading(true);
    try {
      await usersService.deleteUser(deletingUser.id);
      setDeletingUser(null);
      await loadUsers(page, pageSize, searchTerm);
      alert('User deleted successfully');
    } catch (err: any) {
      setError(err?.message || 'Delete failed');
    } finally { setActionLoading(false); }
  };

  return (
    <div className="users-page">
      <section className="users-hero">
        <div className="users-hero-copy">
          <h2>Users Management</h2>
          <p>Search, create, edit, and remove users from one clean screen.</p>
        </div>
        <div className="users-hero-actions">
          {isAdmin && <button className="btn btn-primary" onClick={() => setShowCreate(true)}>Create User</button>}
        </div>
      </section>

      <section className="users-stats" aria-label="Users summary">
        <article className="users-stat-card">
          <span className="users-stat-label">Total users</span>
          <span className="users-stat-value">{totalCount}</span>
        </article>
        <article className="users-stat-card">
          <span className="users-stat-label">Page</span>
          <span className="users-stat-value">{page} of {Math.max(totalPages, 1)}</span>
        </article>
        <article className="users-stat-card">
          <span className="users-stat-label">Showing</span>
          <span className="users-stat-value">{users.length}</span>
        </article>
      </section>

      <section className="users-toolbar">
        <div className="users-search-group">
          <input
            placeholder="Search by email or name..."
            value={search}
            onChange={handleSearchChange}
            aria-label="Search users"
          />
          <button className="btn btn-link" type="button" onClick={handleClearSearch} disabled={!search}>
            Clear
          </button>
        </div>
        <div className="users-toolbar-meta">
          <label htmlFor="users-page-size">Users per page</label>
          <select
            id="users-page-size"
            value={pageSize}
            onChange={(e) => { setPageSize(Number(e.target.value)); setPage(1); }}
            aria-label="Users per page"
          >
            <option value={10}>10</option>
            <option value={25}>25</option>
            <option value={50}>50</option>
          </select>
        </div>
      </section>

      <section className="users-content">
        {loading ? (
          <div className="users-loading" aria-busy="true" aria-live="polite">
            <div className="users-loading-grid">
              <div className="users-skeleton-row" />
              <div className="users-skeleton-row" />
              <div className="users-skeleton-row" />
            </div>
          </div>
        ) : error ? (
          <div className="users-error">
            <h3>Couldn’t load users</h3>
            <p>{error}</p>
            <div className="users-error-actions">
              <button className="btn btn-primary" onClick={() => void loadUsers(page, pageSize, searchTerm)}>Retry</button>
            </div>
          </div>
        ) : users.length === 0 ? (
          <div className="users-empty">
            <h3>No users found</h3>
            <p>{searchTerm ? 'Try a different search term or clear the filter.' : 'There are no users to display yet.'}</p>
            <div className="users-empty-actions">
              {searchTerm && <button className="btn" type="button" onClick={handleClearSearch}>Clear search</button>}
              {isAdmin && <button className="btn btn-primary" type="button" onClick={() => setShowCreate(true)}>Create User</button>}
            </div>
          </div>
        ) : (
          <>
            <UsersList users={users} onEdit={(u) => setEditingUser(u)} onDelete={(u) => setDeletingUser(u)} />

            <div className="users-pagination">
              <div className="users-pagination-info">
                Showing {(page - 1) * pageSize + 1} - {Math.min(page * pageSize, totalCount)} of {totalCount}
              </div>
              <div className="users-pagination-controls">
                <button className="btn" onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page <= 1}>Previous</button>
                <span>Page {page} of {totalPages}</span>
                <button className="btn" onClick={() => setPage((p) => Math.min(totalPages, p + 1))} disabled={page >= totalPages}>Next</button>
              </div>
            </div>
          </>
        )}
      </section>

      {!isAdmin && (
        <div className="users-permission">
          <h3>You don't have permission.</h3>
          <p>Create, edit, and delete actions are available to Admin users only.</p>
        </div>
      )}

      {showCreate && (
        <UserForm onCancel={() => setShowCreate(false)} onSubmit={handleCreate} isAdmin={isAdmin} />
      )}

      {editingUser && (
        <UserForm initial={editingUser} onCancel={() => setEditingUser(null)} onSubmit={handleEdit} isAdmin={isAdmin} />
      )}

      {deletingUser && (
        <DeleteConfirmation message={`Are you sure you want to delete ${deletingUser.email}?`} onCancel={() => setDeletingUser(null)} onConfirm={handleDelete} loading={actionLoading} />
      )}
    </div>
  );
};

export default UsersPage;
