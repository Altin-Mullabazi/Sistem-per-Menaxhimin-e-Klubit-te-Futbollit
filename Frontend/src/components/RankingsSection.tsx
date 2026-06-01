import React from 'react';
import { TopScorer } from '../types';

interface RankingsSectionProps {
  title: string;
  items: TopScorer[];
  metricName: string;
  primaryValueKey: 'goalsScored' | 'assists';
}

const RankingsSection: React.FC<RankingsSectionProps> = ({ title, items, metricName, primaryValueKey }) => {
  return (
    <section className="rankings-section">
      <div className="rankings-header">
        <h2>{title}</h2>
      </div>

      <div className="rankings-table-container">
        <table className="rankings-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Player</th>
              <th>Club</th>
              <th>{metricName}</th>
              <th>Goals</th>
              <th>Assists</th>
            </tr>
          </thead>
          <tbody>
            {items.length === 0 ? (
              <tr>
                <td colSpan={6} className="empty-row">
                  No ranking data available.
                </td>
              </tr>
            ) : (
              items.map((item, index) => (
                <tr key={`${item.playerId}-${index}`}>
                  <td>{index + 1}</td>
                  <td>{item.playerName}</td>
                  <td>{item.clubName || '-'}</td>
                  <td>{item[primaryValueKey]}</td>
                  <td>{item.goalsScored}</td>
                  <td>{item.assists}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </section>
  );
};

export default RankingsSection;
