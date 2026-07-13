import React, { useEffect, useState } from 'react';
import { Icon } from '../components/Icon';
import { stadiumService } from '../services/stadiumService';
import { Stadium } from '../types';
import '../styles/ComingSoon.css';

const Stadiums: React.FC = () => {
  const [stadiums, setStadiums] = useState<Stadium[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadStadiums = async () => {
    setIsLoading(true);
    setError(null);
    try {
      setStadiums(await stadiumService.getStadiums());
    } catch (err: any) {
      setError(err.message || 'Unable to load stadiums.');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    void loadStadiums();
  }, []);

  return (
    <div className="stadiums-page">
      <section className="module-intro">
        <div>
          <span className="module-eyebrow">Club infrastructure</span>
          <h2>Stadium Directory</h2>
          <p>Browse the registered venues available across club operations.</p>
        </div>
        <button className="btn btn-primary" onClick={loadStadiums} disabled={isLoading}>
          {isLoading ? 'Refreshing…' : 'Refresh venues'}
        </button>
      </section>

      {error && <div className="error-message">{error}</div>}
      {isLoading && <div className="loading">Loading stadium directory…</div>}

      {!isLoading && !error && stadiums.length === 0 && (
        <div className="empty-state">No stadiums have been registered yet.</div>
      )}

      {!isLoading && stadiums.length > 0 && (
        <div className="stadium-grid">
          {stadiums.map((stadium, index) => (
            <article className="stadium-card" key={stadium.id}>
              <div className="stadium-visual">
                <span className="stadium-index">{String(index + 1).padStart(2, '0')}</span>
                <Icon name="stadium" size={36} />
              </div>
              <div className="stadium-card-body">
                <span className="status-badge status-active">Operational venue</span>
                <h3>{stadium.name}</h3>
                <p>{stadium.city || 'Location not specified'}</p>
              </div>
            </article>
          ))}
        </div>
      )}
    </div>
  );
};

export default Stadiums;
