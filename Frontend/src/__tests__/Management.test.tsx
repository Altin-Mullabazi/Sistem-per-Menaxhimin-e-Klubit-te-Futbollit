import React from 'react';
import { render, screen } from '@testing-library/react';
import Management from '../pages/Management';
import { AuthProvider } from '../context/AuthContext';
import userEvent from '@testing-library/user-event';

describe('Management page', () => {
  it('renders tabs', () => {
    render(
      <AuthProvider>
        <Management />
      </AuthProvider>
    );

    expect(screen.getByText(/Transfers/i)).toBeInTheDocument();
    expect(screen.getByText(/Contracts/i)).toBeInTheDocument();
    expect(screen.getByText(/Injuries/i)).toBeInTheDocument();
  });

  it('switches tabs and preserves sub-tabs', async () => {
    render(
      <AuthProvider>
        <Management />
      </AuthProvider>
    );

    const user = userEvent.setup();
    const contractsTab = screen.getByText(/Contracts/i);
    await user.click(contractsTab);

    expect(screen.getByText(/Active/i)).toBeInTheDocument();
    expect(screen.getByText(/Expiring/i)).toBeInTheDocument();
  });

  it('shows disabled create buttons when unauthenticated', () => {
    render(
      <AuthProvider>
        <Management />
      </AuthProvider>
    );

    // Create Transfer button exists but should be disabled for unauthenticated users
    const createBtn = screen.getByText(/Create Transfer/i);
    expect(createBtn).toBeDisabled();
  });
});
