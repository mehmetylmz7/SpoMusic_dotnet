using SpoMusic.Api.DTOs;

namespace SpoMusic.Api.Services.Matching;

public interface IMatchingService
{
    Task<List<MatchResultItemDto>> GetMatchesForUserAsync(string userId, int limit = 10);
    Task<CompareUsersResponseDto> CompareUsersAsync(string user1Id, string user2Id);
}
