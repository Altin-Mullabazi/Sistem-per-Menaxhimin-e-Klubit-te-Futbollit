import React from 'react';
import '../styles/LeadSections.css';

type Tone = 'green' | 'blue' | 'orange' | 'slate';

interface BadgeProps {
  tone?: Tone;
  children: React.ReactNode;
}

const Badge: React.FC<BadgeProps> = ({ tone = 'slate', children }) => (
  <span className={`lead-badge lead-badge-${tone}`}>{children}</span>
);

interface SectionProps {
  icon: string;
  title: string;
  badge?: React.ReactNode;
  children: React.ReactNode;
  defaultOpen?: boolean;
}

const Section: React.FC<SectionProps> = ({ icon, title, badge, children, defaultOpen = true }) => (
  <details className="lead-section" open={defaultOpen}>
    <summary>
      <span className="lead-section-icon">{icon}</span>
      <span>{title}</span>
      {badge}
    </summary>
    <div className="lead-section-body">{children}</div>
  </details>
);

const items = {
  mainNavigation: [
    ['DB', 'Dashboard', 'Overview and core club metrics'],
    ['CL', 'Clubs', 'Club records and details'],
    ['PL', 'Players', 'Player data management'],
    ['ST', 'Stadiums', 'Stadium information'],
  ],
  dropdowns: [
    ['DATA', 'Manage Data', 'Clubs, Players, Stadiums'],
    ['MATCH', 'Match System', 'Matches, Events & Stats'],
    ['MGT', 'Management', 'Transfers, Contracts, Injuries'],
    ['ADM', 'Admin', 'Users, Sponsors & Seasons'],
  ],
  authPages: [
    ['Login Page', 'Email and password sign-in with loading, validation, and error states.'],
    ['Register Page', 'First name, last name, email, password, and confirm password registration flow.'],
  ],
};

const LeadSections: React.FC = () => {
  return (
    <div className="lead-page">
      <div className="lead-page-header">
        <div>
          <p className="lead-eyebrow">P5 Lead sections</p>
          <h1>Frontend System Cards</h1>
        </div>
        <Badge tone="blue">Admin dashboard UI</Badge>
      </div>

      <div className="lead-card-grid">
        <article className="lead-card">
          <header className="lead-card-header">
            <div className="lead-title-wrap">
              <span className="lead-card-icon">NAV</span>
              <div>
                <h2>Navigation Bar</h2>
                <p>Responsive application navigation for dashboard, management, match, and admin workflows.</p>
              </div>
            </div>
            <div className="lead-header-badges">
              <Badge tone="green">Required</Badge>
              <Badge tone="green">Working</Badge>
            </div>
          </header>

          <div className="lead-content-grid">
            <Section icon="MAP" title="Main Navigation" badge={<Badge tone="blue">Core links</Badge>}>
              <div className="lead-mini-grid">
                {items.mainNavigation.map(([icon, label, detail]) => (
                  <div className="lead-link-tile" key={label}>
                    <span>{icon}</span>
                    <div>
                      <strong>{label}</strong>
                      <p>{detail}</p>
                    </div>
                  </div>
                ))}
              </div>
            </Section>

            <Section icon="MEN" title="Dropdown Menus" badge={<Badge tone="blue">Grouped</Badge>}>
              <div className="lead-stack">
                {items.dropdowns.map(([icon, label, detail]) => (
                  <div className="lead-row" key={label}>
                    <span className="lead-pill-icon">{icon}</span>
                    <div>
                      <strong>{label}</strong>
                      <p>{detail}</p>
                    </div>
                  </div>
                ))}
              </div>
            </Section>

            <Section icon="USR" title="User Menu" badge={<Badge tone="green">Working</Badge>}>
              <div className="lead-check-list">
                <span>User avatar, username, and role summary</span>
                <span>Profile shortcut</span>
                <span>Logout action with redirect to login</span>
              </div>
            </Section>

            <Section icon="MOB" title="Mobile Menu" badge={<Badge tone="green">Responsive</Badge>}>
              <div className="lead-check-list">
                <span>Hamburger trigger</span>
                <span>Slide-style compact menu structure</span>
                <span>Mobile dropdown groups</span>
                <span>Route changes close the menu</span>
              </div>
            </Section>

            <Section icon="FEA" title="Features" badge={<Badge tone="blue">Checklist</Badge>}>
              <div className="lead-check-grid">
                <Badge tone="green">Role-aware links</Badge>
                <Badge tone="green">Active route states</Badge>
                <Badge tone="green">Click outside closes menus</Badge>
                <Badge tone="green">Desktop and mobile layouts</Badge>
                <Badge tone="orange">Admin Only sections</Badge>
                <Badge tone="blue">Grouped navigation</Badge>
              </div>
            </Section>

            <Section icon="RUN" title="Functionality" badge={<Badge tone="green">Status</Badge>}>
              <div className="lead-status-grid">
                <div><span className="status-dot green"></span>Authenticated users see navigation</div>
                <div><span className="status-dot green"></span>Managers see management links</div>
                <div><span className="status-dot orange"></span>Admin menu is restricted</div>
                <div><span className="status-dot green"></span>Logout clears session flow</div>
              </div>
            </Section>

            <Section icon="QA" title="Testing Checklist" badge={<Badge tone="blue">Compact</Badge>} defaultOpen={false}>
              <div className="lead-test-grid">
                <span>Desktop nav</span>
                <span>Mobile nav</span>
                <span>Dropdown open/close</span>
                <span>Role visibility</span>
                <span>Profile menu</span>
                <span>Logout redirect</span>
              </div>
            </Section>
          </div>
        </article>

        <article className="lead-card">
          <header className="lead-card-header">
            <div className="lead-title-wrap">
              <span className="lead-card-icon">SEC</span>
              <div>
                <h2>Authentication System</h2>
                <p>Login, registration, protected routing, token handling, and session recovery for the frontend.</p>
              </div>
            </div>
            <div className="lead-header-badges">
              <Badge tone="green">Required</Badge>
              <Badge tone="orange">Security</Badge>
            </div>
          </header>

          <div className="lead-content-grid auth-grid">
            <Section icon="IN" title="Login Page" badge={<Badge tone="green">Working</Badge>}>
              <div className="lead-form-preview">
                <span>Email input</span>
                <span>Password input</span>
                <span>Loading state</span>
                <span>Error message</span>
              </div>
            </Section>

            <Section icon="REG" title="Register Page" badge={<Badge tone="green">Working</Badge>}>
              <div className="lead-form-preview">
                <span>First name</span>
                <span>Last name</span>
                <span>Email</span>
                <span>Password confirmation</span>
              </div>
            </Section>

            <Section icon="RTE" title="Protected Routes" badge={<Badge tone="orange">Auth required</Badge>}>
              <div className="lead-flow">
                <span>Route request</span>
                <strong>Auth check</strong>
                <span>Protected page</span>
              </div>
              <p className="lead-note">Unauthenticated users are redirected to the login page.</p>
            </Section>

            <Section icon="TOK" title="Token Management" badge={<Badge tone="blue">Session</Badge>}>
              <div className="lead-check-list">
                <span>Stores access token</span>
                <span>Stores refresh token</span>
                <span>Applies bearer token to API requests</span>
                <span>Clears tokens on logout</span>
              </div>
            </Section>

            <Section icon="API" title="Axios Interceptors" badge={<Badge tone="blue">API client</Badge>}>
              <div className="lead-status-grid">
                <div><span className="status-dot green"></span>Request interceptor adds authorization header</div>
                <div><span className="status-dot green"></span>Response interceptor handles unauthorized responses</div>
                <div><span className="status-dot green"></span>Refresh flow retries the original request</div>
              </div>
            </Section>

            <Section icon="OUT" title="Auto Logout Logic" badge={<Badge tone="orange">Session note</Badge>}>
              <div className="lead-check-list">
                <span>Expired sessions are detected through API responses</span>
                <span>Refresh failure clears stored tokens</span>
                <span>User returns to login when authentication is no longer valid</span>
              </div>
            </Section>

            <Section icon="OK" title="Functionality Checklist" badge={<Badge tone="green">Success</Badge>} defaultOpen={false}>
              <div className="lead-check-grid">
                <Badge tone="green">Login success</Badge>
                <Badge tone="green">Register success</Badge>
                <Badge tone="green">ProtectedRoute gate</Badge>
                <Badge tone="green">Token refresh</Badge>
                <Badge tone="green">Logout cleanup</Badge>
                <Badge tone="orange">Invalid session redirect</Badge>
              </div>
            </Section>
          </div>
        </article>
      </div>
    </div>
  );
};

export default LeadSections;
