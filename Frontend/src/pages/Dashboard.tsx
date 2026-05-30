import React, { useEffect, useState } from 'react';
import { dashboardService } from '../services/dashboardService';
import {
  DashboardSummary,
  ExpiringContract,
  InjuredPlayer,
  RecentTransfer,
  TopScorer,
  UpcomingMatch,
} from '../types';
import '../styles/Dashboard.css';

const formatDate = (dateString: string) => new Date(dateString).toLocaleDateString();

const getUrgencyClass = (endDate: string) => {
  const days = Math.ceil((new Date(endDate).getTime() - Date.now()) / (1000 * 60 * 60 * 24));
  if (days <= 7) return 'urgency-high';
  if (days <= 30) return 'urgency-medium';
  return '';
};

const Dashboard: React.FC = () => {
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [upcomingMatches, setUpcomingMatches] = useState<UpcomingMatch[]>([]);
  const [topScorers, setTopScorers] = useState<TopScorer[]>([]);
  const [injuredPlayers, setInjuredPlayers] = useState<InjuredPlayer[]>([]);
  const [expiringContracts, setExpiringContracts] = useState<ExpiringContract[]>([]);
  const [recentTransfers, setRecentTransfers] = useState<RecentTransfer[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadDashboard = async () => {
    setIsLoading(true);
    setError(null);

    try {
      const [summaryData, matchesData, scorersData, injuredData, contractsData, transfersData] = await Promise.all([
        dashboardService.getSummary(),
        dashboardService.getUpcomingMatches(),
        dashboardService.getTopScorers(),
        dashboardService.getInjuredPlayers(),
        dashboardService.getExpiringContracts(),
        dashboardService.getRecentTransfers(),
      ]);

      setSummary(summaryData);
      setUpcomingMatches(matchesData.sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime()));
      setTopScorers(scorersData.slice(0, 10));
      setInjuredPlayers(injuredData);
      setExpiringContracts(contractsData.sort((a, b) => new Date(a.endDate).getTime() - new Date(b.endDate).getTime()));
      setRecentTransfers(transfersData);
    } catch (err: any) {
      setError(err.message || 'Unable to load dashboard data.');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadDashboard();
  }, []);

  return (
    <div className="dashboard-page">
      <div className="dashboard-header">
        <div>
          <h1>Club Dashboard</h1>
          <p>Key performance data across clubs, players, matches, and contracts.</p>
        </div>
        <button className="btn btn-primary" onClick={loadDashboard} disabled={isLoading}>
          Refresh
        </button>
      </div>

      {error && <div className="error-message">{error}</div>}

      {isLoading && (
        <div className="dashboard-loading">
          <div className="spinner" aria-hidden="true" />
          Loading dashboard data...
        </div>
      )}

      {!isLoading && summary && (
        <section className="summary-grid">
          <div className="summary-card card-primary">
            <div className="summary-card-icon">🏟️</div>
            <div>
              <p>Total Clubs</p>
              <h2>{summary.totalClubs}</h2>
            </div>
          </div>
          <div className="summary-card card-secondary">
            <div className="summary-card-icon">👥</div>
            <div>
              <p>Total Players</p>
              <h2>{summary.totalPlayers}</h2>
            </div>
          </div>
          <div className="summary-card card-info">
            <div className="summary-card-icon">⚽</div>
            <div>
              <p>Total Matches</p>
              <h2>{summary.totalMatches}</h2>
            </div>
          </div>
          <div className="summary-card card-success">
            <div className="summary-card-icon">🧑‍💼</div>
            <div>
              <p>Total Staff</p>
              <h2>{summary.totalStaff}</h2>
            </div>
          </div>
          <div className="summary-card card-warning">
            <div className="summary-card-icon">🩹</div>
            <div>
              <p>Total Injuries</p>
              <h2>{summary.totalInjuries}</h2>
            </div>
          </div>
          <div className="summary-card card-danger">
            <div className="summary-card-icon">📜</div>
            <div>
              <p>Total Contracts</p>
              <h2>{summary.totalContracts}</h2>
            </div>
          </div>
        </section>
      )}

      <section className="dashboard-grid">
        <div className="dashboard-card section-card">
          <div className="section-header">
            <div>
              <h2>Upcoming Matches</h2>
              <p>Next 7 days, sorted by date.</p>
            </div>
          </div>
          {upcomingMatches.length === 0 ? (
            <div className="empty-state">No upcoming matches in the next week.</div>
          ) : (
            <div className="table-scroll">
              <table>
                <thead>
                  <tr>
                    <th>Date</th>
                    <th>Teams</th>
                    <th>Stadium</th>
                  </tr>
                </thead>
                <tbody>
                  {upcomingMatches.map((match) => (
                    <tr key={match.id}>
                      <td>{formatDate(match.date)}</td>
                      <td>{match.homeTeam} vs {match.awayTeam}</td>
                      <td>{match.stadium}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>

        <div className="dashboard-card section-card">
          <div className="section-header">
            <div>
              <h2>Top Scorers</h2>
              <p>Top 10 player rankings by goals and assists.</p>
            </div>
          </div>
          {topScorers.length === 0 ? (
            <div className="empty-state">No scoring data available.</div>
          ) : (
            <div className="table-scroll">
              <table>
                <thead>
                  <tr>
                    <th>#</th>
                    <th>Player</th>
                    <th>Club</th>
                    <th>Goals</th>
                    <th>Assists</th>
                  </tr>
                </thead>
                <tbody>
                  {topScorers.map((scorer, index) => (
                    <tr key={`${scorer.playerName}-${index}`}>
                      <td>{index + 1}</td>
                      <td>{scorer.playerName}</td>
                      <td>{scorer.clubName}</td>
                      <td>{scorer.goals}</td>
                      <td>{scorer.assists}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>

        <div className="dashboard-card section-card">
          <div className="section-header">
            <div>
              <h2>Injured Players</h2>
              <p>Players with active injuries and recovery windows.</p>
            </div>
          </div>
          {injuredPlayers.length === 0 ? (
            <div className="empty-state">No active injuries at the moment.</div>
          ) : (
            <div className="table-scroll">
              <table>
                <thead>
                  <tr>
                    <th>Player</th>
                    <th>Injury</th>
                    <th>From</th>
                    <th>To</th>
                  </tr>
                </thead>
                <tbody>
                  {injuredPlayers.map((injury) => (
                    <tr key={injury.id}>
                      <td>{injury.playerName}</td>
                      <td>{injury.injury}</td>
                      <td>{formatDate(injury.startDate)}</td>
                      <td>{formatDate(injury.endDate)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </section>

      <section className="dashboard-grid">
        <div className="dashboard-card section-card">
          <div className="section-header">
            <div>
              <h2>Expiring Contracts</h2>
              <p>Contracts ending in the next 90 days.</p>
            </div>
          </div>
          {expiringContracts.length === 0 ? (
            <div className="empty-state">No contracts are expiring soon.</div>
          ) : (
            <div className="table-scroll">
              <table>
                <thead>
                  <tr>
                    <th>Player</th>
                    <th>Club</th>
                    <th>End Date</th>
                    <th>Urgency</th>
                  </tr>
                </thead>
                <tbody>
                  {expiringContracts.map((contract) => (
                    <tr key={contract.id} className={getUrgencyClass(contract.endDate)}>
                      <td>{contract.playerName}</td>
                      <td>{contract.clubName}</td>
                      <td>{formatDate(contract.endDate)}</td>
                      <td>{Math.ceil((new Date(contract.endDate).getTime() - Date.now()) / (1000 * 60 * 60 * 24))} days</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>

        <div className="dashboard-card section-card">
          <div className="section-header">
            <div>
              <h2>Recent Transfers</h2>
              <p>Transfers completed in the last 30 days.</p>
            </div>
          </div>
          {recentTransfers.length === 0 ? (
            <div className="empty-state">No recent transfers recorded.</div>
          ) : (
            <div className="table-scroll">
              <table>
                <thead>
                  <tr>
                    <th>Player</th>
                    <th>From</th>
                    <th>To</th>
                    <th>Fee</th>
                  </tr>
                </thead>
                <tbody>
                  {recentTransfers.map((transfer) => (
                    <tr key={transfer.id}>
                      <td>{transfer.playerName}</td>
                      <td>{transfer.fromClubName}</td>
                      <td>{transfer.toClubName}</td>
                      <td>{transfer.fee}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>

        <div className="dashboard-card section-card section-empty">
          <div className="section-header">
            <div>
              <h2>Overview</h2>
              <p>Use the dashboard to monitor upcoming action and contract risk.</p>
            </div>
          </div>
          <div className="empty-state">
            <p>Pull fresh data with the Refresh button.</p>
          </div>
        </div>
      </section>
    </div>
  );
};

export default Dashboard;
