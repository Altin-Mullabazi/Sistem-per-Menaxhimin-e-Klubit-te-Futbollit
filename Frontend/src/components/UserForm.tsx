import React, { useState, useEffect } from 'react';
import { User } from '../types';
import '../styles/UsersPage.css';

interface Props {
  initial?: User | null;
  onCancel: () => void;
  onSubmit: (data: { email: string; password?: string; confirmPassword?: string; firstName: string; lastName: string; role?: string }) => Promise<void>;
  isAdmin: boolean;
}

const roleOptions = ['Admin', 'Manager', 'Coach', 'User'];

const UserForm: React.FC<Props> = ({ initial = null, onCancel, onSubmit, isAdmin }) => {
  const [email, setEmail] = useState(initial?.email || '');
  const [firstName, setFirstName] = useState(initial?.firstName || '');
  const [lastName, setLastName] = useState(initial?.lastName || '');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [role, setRole] = useState(roleOptions[3]);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (initial) {
      setEmail(initial.email);
      setFirstName(initial.firstName ?? '');
      setLastName(initial.lastName ?? '');
    }
  }, [initial]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!email || !firstName || !lastName) {
      setError('Please fill required fields');
      return;
    }

    if (!initial) {
      if (!password || password.length < 8) {
        setError('Password must be at least 8 characters');
        return;
      }
      if (password !== confirmPassword) {
        setError('Passwords do not match');
        return;
      }
    }

    setSubmitting(true);
    try {
      await onSubmit({ email, password: password || undefined, confirmPassword: confirmPassword || undefined, firstName, lastName, role });
    } catch (err: any) {
      setError(err?.message || 'An error occurred');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="users-modal-overlay" role="presentation" onClick={onCancel}>
      <div className="users-modal" role="dialog" aria-modal="true" aria-label={initial ? 'Edit user dialog' : 'Create user dialog'} onClick={(event) => event.stopPropagation()}>
        <div className="users-modal-header">
          <div>
            <h3>{initial ? 'Edit User' : 'Create User'}</h3>
            <p>{initial ? 'Update the user profile details.' : 'Add a new team member to the system.'}</p>
          </div>
          <button type="button" className="users-modal-close" onClick={onCancel} aria-label="Close dialog">×</button>
        </div>

        <form className="users-form" onSubmit={handleSubmit}>
          {error && <div className="users-inline-error">{error}</div>}

          <div className="users-form-grid">
            <div className="users-field users-field-full">
              <label htmlFor="user-email">Email</label>
              <input id="user-email" type="email" value={email} onChange={(e) => setEmail(e.target.value)} disabled={!!initial} placeholder="Enter email" />
            </div>

            <div className="users-field">
              <label htmlFor="user-first-name">First Name</label>
              <input id="user-first-name" value={firstName} onChange={(e) => setFirstName(e.target.value)} placeholder="Enter first name" />
            </div>

            <div className="users-field">
              <label htmlFor="user-last-name">Last Name</label>
              <input id="user-last-name" value={lastName} onChange={(e) => setLastName(e.target.value)} placeholder="Enter last name" />
            </div>

            {!initial && (
              <>
                <div className="users-field">
                  <label htmlFor="user-password">Password</label>
                  <input id="user-password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="At least 8 characters" />
                </div>

                <div className="users-field">
                  <label htmlFor="user-confirm-password">Confirm Password</label>
                  <input id="user-confirm-password" type="password" value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} placeholder="Repeat password" />
                </div>
              </>
            )}

            {!initial && isAdmin && (
              <div className="users-field users-field-full">
                <label htmlFor="user-role">Role</label>
                <select id="user-role" value={role} onChange={(e) => setRole(e.target.value)} aria-label="Role">
                  {roleOptions.map((r) => (
                    <option key={r} value={r}>{r}</option>
                  ))}
                </select>
                <span className="users-helper">Choose the appropriate permissions level.</span>
              </div>
            )}
          </div>

          <div className="users-form-actions">
            <button type="button" className="btn" onClick={onCancel} disabled={submitting}>Cancel</button>
            <button type="submit" className="btn btn-primary" disabled={submitting}>{submitting ? 'Saving...' : 'Submit'}</button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default UserForm;
