using SpoMusic.Api.Services.Matching;
using Xunit;

namespace SpoMusic.Tests;

public class MatchingEngineTests
{
    private UserProfile CreateProfile(
        string userId = "user-1",
        string spotifyId = "spotify-1",
        List<string>? topTracks = null,
        List<string>? topArtists = null,
        List<ListeningHistoryItem>? listeningHistory = null)
    {
        return new UserProfile(
            userId,
            spotifyId,
            topTracks ?? new List<string> { "track-1", "track-2", "track-3", "track-4", "track-5" },
            topArtists ?? new List<string> { "artist-1", "artist-2", "artist-3" },
            listeningHistory ?? new List<ListeningHistoryItem>
            {
                new("track-1", DateTime.UtcNow.AddDays(-5)),
                new("track-2", DateTime.UtcNow.AddDays(-10)),
                new("track-3", DateTime.UtcNow.AddDays(-15))
            }
        );
    }

    [Fact]
    public void CalculateScore_ReturnsHighScore_ForUsersWithManyCommonTracks()
    {
        var profileA = CreateProfile();
        var profileB = CreateProfile(
            userId: "user-2",
            topTracks: new List<string> { "track-1", "track-2", "track-3", "track-4", "track-5" },
            topArtists: new List<string> { "artist-1", "artist-2", "artist-3" }
        );

        var score = MatchingEngine.CalculateScore(profileA, profileB);

        Assert.Equal(5, score.CommonTracks);
        Assert.Equal(3, score.CommonArtists);
        Assert.True(score.Total > 0.5);
    }

    [Fact]
    public void CalculateScore_ReturnsZeroCommonTracks_ForCompletelyDifferentUsers()
    {
        var profileA = CreateProfile(
            topTracks: new List<string> { "track-1", "track-2" },
            topArtists: new List<string> { "artist-1" }
        );
        var profileB = CreateProfile(
            userId: "user-2",
            topTracks: new List<string> { "track-3", "track-4" },
            topArtists: new List<string> { "artist-2" }
        );

        var score = MatchingEngine.CalculateScore(profileA, profileB);

        Assert.Equal(0, score.CommonTracks);
        Assert.Equal(0, score.CommonArtists);
        Assert.True(score.Total < 0.35);
    }

    [Fact]
    public void CalculateScore_RecentListeningOverlap_ProducesPositiveRecencyScore()
    {
        var now = DateTime.UtcNow;
        var profileA = CreateProfile(
            listeningHistory: new List<ListeningHistoryItem>
            {
                new("track-1", now.AddMinutes(-5))
            }
        );
        var profileB = CreateProfile(
            userId: "user-2",
            listeningHistory: new List<ListeningHistoryItem>
            {
                new("track-1", now.AddMinutes(-10))
            }
        );

        var score = MatchingEngine.CalculateScore(profileA, profileB);
        Assert.True(score.RecencyScore > 0.5);
    }

    [Fact]
    public void CalculateScore_PartialOverlap_CalculatesProportionalScore()
    {
        var profileA = CreateProfile(
            topTracks: new List<string> { "t1", "t2", "t3", "t4" },
            topArtists: new List<string> { "a1", "a2" }
        );
        var profileB = CreateProfile(
            userId: "user-2",
            topTracks: new List<string> { "t3", "t4", "t5", "t6" },
            topArtists: new List<string> { "a2", "a3" }
        );

        var score = MatchingEngine.CalculateScore(profileA, profileB);

        Assert.Equal(2, score.CommonTracks);
        Assert.Equal(1, score.CommonArtists);
        Assert.True(score.Total is > 0.2 and < 0.9);
    }
}
