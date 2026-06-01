import React from 'react';

interface MatchFiltersProps {
  clubId?: number;
  seasonId?: number;
  status?: string;
  startDate?: string;
  endDate?: string;
  page: number;
  pageSize: number;
  totalPages: number;
  clubs: any[];
  seasons: any[];
  onClubChange: (clubId?: number) => void;
  onSeasonChange: (seasonId?: number) => void;
  onStatusChange: (status?: string) => void;
  onStartDateChange: (date?: string) => void;
  onEndDateChange: (date?: string) => void;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
}

const MatchFilters: React.FC<MatchFiltersProps> = ({
  clubId,
  seasonId,
  status,
  startDate,
  endDate,
  page,
  pageSize,
  totalPages,
  clubs,
  seasons,
  onClubChange,
  onSeasonChange,
  onStatusChange,
  onStartDateChange,
  onEndDateChange,
  onPageChange,
  onPageSizeChange,
}) => {
  const statuses = ['Scheduled', 'Ongoing', 'Completed'];

  return (
    <div className="filters-section">
      <div className="filters-grid">
        <div className="filter-group">
          <label htmlFor="clubFilter">Filter by Club:</label>
          <select
            id="clubFilter"
            value={clubId || ''}
            onChange={(e) => {
              onClubChange(e.target.value ? parseInt(e.target.value) : undefined);
              onPageChange(1);
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
          <label htmlFor="seasonFilter">Filter by Season:</label>
          <select
            id="seasonFilter"
            value={seasonId || ''}
            onChange={(e) => {
              onSeasonChange(e.target.value ? parseInt(e.target.value) : undefined);
              onPageChange(1);
            }}
          >
            <option value="">All Seasons</option>
            {seasons.map((season) => (
              <option key={season.id} value={season.id}>
                {season.name}
              </option>
            ))}
          </select>
        </div>

        <div className="filter-group">
          <label htmlFor="statusFilter">Filter by Status:</label>
          <select
            id="statusFilter"
            value={status || ''}
            onChange={(e) => {
              onStatusChange(e.target.value || undefined);
              onPageChange(1);
            }}
          >
            <option value="">All Statuses</option>
            {statuses.map((s) => (
              <option key={s} value={s}>
                {s}
              </option>
            ))}
          </select>
        </div>

        <div className="filter-group">
          <label htmlFor="startDateFilter">Start Date:</label>
          <input
            id="startDateFilter"
            type="date"
            value={startDate || ''}
            onChange={(e) => {
              onStartDateChange(e.target.value || undefined);
              onPageChange(1);
            }}
          />
        </div>

        <div className="filter-group">
          <label htmlFor="endDateFilter">End Date:</label>
          <input
            id="endDateFilter"
            type="date"
            value={endDate || ''}
            onChange={(e) => {
              onEndDateChange(e.target.value || undefined);
              onPageChange(1);
            }}
          />
        </div>
      </div>

      <div className="pagination-section">
        <div className="pagination-controls">
          <button
            className="btn btn-sm"
            onClick={() => onPageChange(page - 1)}
            disabled={page === 1}
          >
            Previous
          </button>

          <div className="page-info">
            Page <input
              type="number"
              min="1"
              max={totalPages}
              value={page}
              onChange={(e) => {
                const newPage = Math.max(1, Math.min(totalPages, parseInt(e.target.value) || 1));
                onPageChange(newPage);
              }}
              className="page-input"
            /> of {totalPages}
          </div>

          <button
            className="btn btn-sm"
            onClick={() => onPageChange(page + 1)}
            disabled={page === totalPages || totalPages === 0}
          >
            Next
          </button>

          <select
            value={pageSize}
            onChange={(e) => {
              onPageSizeChange(parseInt(e.target.value));
              onPageChange(1);
            }}
            className="page-size-select"
          >
            <option value="5">5 per page</option>
            <option value="10">10 per page</option>
            <option value="20">20 per page</option>
            <option value="50">50 per page</option>
          </select>
        </div>
      </div>
    </div>
  );
};

export default MatchFilters;
