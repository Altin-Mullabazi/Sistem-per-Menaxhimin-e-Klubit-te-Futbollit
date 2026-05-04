using FootballClubAPI.DTOs;

namespace FootballClubAPI.Services
{
    public interface IClubService
    {
        /// <summary>
        /// Retrieve all clubs with pagination, search, filter and sort
        /// </summary>
        Task<ClubListResponseDto> GetAllClubsAsync(
            int page = 1, 
            int pageSize = 10, 
            string? search = null, 
            string? city = null, 
            string sortBy = "name");

        /// <summary>
        /// Retrieve a specific club by ID with players
        /// </summary>
        Task<ClubDetailDto?> GetClubByIdAsync(int id);

        /// <summary>
        /// Create a new club with validation
        /// </summary>
        Task<ClubDto> CreateClubAsync(CreateClubDto createClubDto);

        /// <summary>
        /// Update an existing club
        /// </summary>
        Task<ClubDto?> UpdateClubAsync(int id, UpdateClubDto updateClubDto);

        /// <summary>
        /// Delete a club by ID
        /// </summary>
        Task<bool> DeleteClubAsync(int id);

        /// <summary>
        /// Get total count of clubs
        /// </summary>
        Task<int> GetClubsCountAsync();
    }
}
