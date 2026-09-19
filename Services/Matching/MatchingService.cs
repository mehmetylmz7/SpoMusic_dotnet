using Microsoft.EntityFrameworkCore;
using SpoMusic.Api.Data;
using SpoMusic.Api.DTOs;
using SpoMusic.Api.Entities;

namespace SpoMusic.Api.Services.Matching;

public class MatchingService : IMatchingService
{
    private readonly AppDbContext _db;
    private readonly ILogger<MatchingService> _logger;

    public MatchingService(AppDbContext db, ILogger<MatchingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<MatchResultItemDto>> GetMatchesForUserAsync(string userId, int limit = 10)
    {
        var oneDayAgo = DateTime.UtcNow.AddDays(-1);

        // 1. Check 24-hour cache
        var cachedMatches = await _db.Matches
            .Where(m => m.User1Id == userId && m.CreatedAt >= oneDayAgo)
            .Include(m => m.User2)
            .OrderByDescending(m => m.MatchScore)
            .Take(limit)
            .ToListAsync();

        if (cachedMatches.Count > 0)
        {
            _logger.LogInformation("Serving {Count} cached matches for user {UserId}", cachedMatches.Count, userId);
            return cachedMatches.Select(m => new MatchResultItemDto(
                new UserSummaryDto(m.User2.Id, m.User2.Name, m.User2.Image, m.User2.Email),
                new MatchScoreDto(m.CommonTracks, m.CommonArtists, 0.8, 0.8, m.MatchScore)
            )).ToList();
        }

        // 2. Fetch target user and candidates
        var targetUser = await _db.Users
            .Include(u => u.TopTracks).ThenInclude(tt => tt.Track)
            .Include(u => u.ListeningHistories).ThenInclude(lh => lh.Track)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (targetUser == null)
        {
            throw new KeyNotFoundException("User profile not found");
        }

        var candidateUsers = await _db.Users
            .Where(u => u.Id != userId)
            .Include(u => u.TopTracks).ThenInclude(tt => tt.Track)
            .Include(u => u.ListeningHistories).ThenInclude(lh => lh.Track)
            .ToListAsync();

        if (candidateUsers.Count == 0)
        {
            return new List<MatchResultItemDto>();
        }

        var targetProfile = MapToProfile(targetUser);
        var results = new List<MatchResultItemDto>();

        foreach (var candidate in candidateUsers)
        {
            var candidateProfile = MapToProfile(candidate);
            var scoreResult = MatchingEngine.CalculateScore(targetProfile, candidateProfile);

            // Find common tracks details
            var commonTrackIds = targetProfile.TopTracks
                .Intersect(candidateProfile.TopTracks, StringComparer.OrdinalIgnoreCase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var commonTrackDetails = targetUser.TopTracks
                .Where(tt => commonTrackIds.Contains(tt.TrackId) && tt.Track != null)
                .Select(tt => new CommonTrackDto(
                    tt.Track!.SpotifyId,
                    tt.Track.Name,
                    tt.Track.Artists.FirstOrDefault() ?? "Unknown",
                    tt.Track.Album,
                    tt.Track.ImageUrl,
                    tt.Track.Url
                ))
                .ToList();

            var commonArtistNames = targetProfile.TopArtists
                .Intersect(candidateProfile.TopArtists, StringComparer.OrdinalIgnoreCase)
                .ToList();

            results.Add(new MatchResultItemDto(
                new UserSummaryDto(candidate.Id, candidate.Name, candidate.Image, candidate.Email),
                new MatchScoreDto(scoreResult.CommonTracks, scoreResult.CommonArtists, scoreResult.RecencyScore, scoreResult.TopSimilarity, scoreResult.Total),
                commonTrackDetails,
                commonArtistNames
            ));
        }

        results = results.OrderByDescending(r => r.Score.Total).Take(limit).ToList();

        // Save / update cache in database
        foreach (var res in results)
        {
            var existingMatch = await _db.Matches
                .FirstOrDefaultAsync(m => m.User1Id == userId && m.User2Id == res.User.Id);

            if (existingMatch != null)
            {
                existingMatch.MatchScore = res.Score.Total;
                existingMatch.CommonTracks = res.Score.CommonTracks;
                existingMatch.CommonArtists = res.Score.CommonArtists;
                existingMatch.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                _db.Matches.Add(new Match
                {
                    User1Id = userId,
                    User2Id = res.User.Id,
                    MatchScore = res.Score.Total,
                    CommonTracks = res.Score.CommonTracks,
                    CommonArtists = res.Score.CommonArtists,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _db.SaveChangesAsync();
        return results;
    }

    public async Task<CompareUsersResponseDto> CompareUsersAsync(string user1Id, string user2Id)
    {
        var users = await _db.Users
            .Where(u => u.Id == user1Id || u.Id == user2Id)
            .Include(u => u.TopTracks).ThenInclude(tt => tt.Track)
            .Include(u => u.ListeningHistories).ThenInclude(lh => lh.Track)
            .ToListAsync();

        var u1 = users.FirstOrDefault(u => u.Id == user1Id);
        var u2 = users.FirstOrDefault(u => u.Id == user2Id);

        if (u1 == null || u2 == null)
        {
            throw new KeyNotFoundException("One or both users not found");
        }

        var profile1 = MapToProfile(u1);
        var profile2 = MapToProfile(u2);
        var scoreResult = MatchingEngine.CalculateScore(profile1, profile2);

        var commonTrackIds = profile1.TopTracks
            .Intersect(profile2.TopTracks, StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var commonTrackDetails = u1.TopTracks
            .Where(tt => commonTrackIds.Contains(tt.TrackId) && tt.Track != null)
            .Select(tt => new CommonTrackDto(
                tt.Track!.SpotifyId,
                tt.Track.Name,
                tt.Track.Artists.FirstOrDefault() ?? "Unknown",
                tt.Track.Album,
                tt.Track.ImageUrl,
                tt.Track.Url
            ))
            .ToList();

        var commonArtistNames = profile1.TopArtists
            .Intersect(profile2.TopArtists, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new CompareUsersResponseDto(
            new UserSummaryDto(u1.Id, u1.Name, u1.Image, u1.Email),
            new UserSummaryDto(u2.Id, u2.Name, u2.Image, u2.Email),
            new MatchScoreDto(scoreResult.CommonTracks, scoreResult.CommonArtists, scoreResult.RecencyScore, scoreResult.TopSimilarity, scoreResult.Total),
            commonTrackDetails,
            commonArtistNames
        );
    }

    private static UserProfile MapToProfile(User user)
    {
        var topTracks = user.TopTracks.Select(t => t.TrackId).ToList();
        var artistSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var tt in user.TopTracks)
        {
            if (tt.Track?.Artists != null)
            {
                foreach (var a in tt.Track.Artists) artistSet.Add(a);
            }
        }

        foreach (var lh in user.ListeningHistories)
        {
            if (lh.Track?.Artists != null)
            {
                foreach (var a in lh.Track.Artists) artistSet.Add(a);
            }
        }

        return new UserProfile(
            user.Id,
            user.SpotifyId ?? user.Email,
            topTracks,
            artistSet.ToList(),
            user.ListeningHistories.Select(lh => new ListeningHistoryItem(lh.TrackId, lh.PlayedAt)).ToList()
        );
    }
}
