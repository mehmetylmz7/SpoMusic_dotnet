using SpoMusic.Api.Services.Spotify;
using Xunit;

namespace SpoMusic.Tests;

public class ActivePlaybackTrackerTests
{
    [Fact]
    public void CoListeners_AreDetected_WhenUsersListenToSameTrack()
    {
        var tracker = new ActivePlaybackTracker();
        var now = DateTime.UtcNow;

        tracker.RecordActivePlayback(new ActiveUserPlayback(
            "user-a",
            "User A",
            "https://example.com/a.jpg",
            "track-123",
            "Song Alpha",
            new List<string> { "Artist 1" },
            "https://example.com/cover.jpg",
            now
        ));

        tracker.RecordActivePlayback(new ActiveUserPlayback(
            "user-b",
            "User B",
            "https://example.com/b.jpg",
            "track-123",
            "Song Alpha",
            new List<string> { "Artist 1" },
            "https://example.com/cover.jpg",
            now
        ));

        tracker.RecordActivePlayback(new ActiveUserPlayback(
            "user-c",
            "User C",
            null,
            "track-999",
            "Song Beta",
            new List<string> { "Artist 2" },
            null,
            now
        ));

        var coListenersA = tracker.GetCoListeners("user-a", "track-123");
        Assert.Single(coListenersA);
        Assert.Equal("user-b", coListenersA[0].UserId);
        Assert.Equal("User B", coListenersA[0].UserName);

        var coListenersB = tracker.GetCoListeners("user-b", "track-123");
        Assert.Single(coListenersB);
        Assert.Equal("user-a", coListenersB[0].UserId);

        var coListenersC = tracker.GetCoListeners("user-c", "track-999");
        Assert.Empty(coListenersC);
    }
}