using System.Collections.Concurrent;

namespace SpoMusic.Api.Services.Spotify;

public class ActivePlaybackTracker : IActivePlaybackTracker
{
    private readonly ConcurrentDictionary<string, ActiveUserPlayback> _activePlaybacks = new();
    private static readonly TimeSpan InactivityTimeout = TimeSpan.FromSeconds(45);

    public void RecordActivePlayback(ActiveUserPlayback playback)
    {
        _activePlaybacks[playback.UserId] = playback;
        PruneExpired();
    }

    public List<ActiveUserPlayback> GetCoListeners(string currentUserId, string trackId)
    {
        PruneExpired();
        var now = DateTime.UtcNow;

        return _activePlaybacks.Values
            .Where(p => p.UserId != currentUserId && 
                        string.Equals(p.TrackId, trackId, StringComparison.OrdinalIgnoreCase) &&
                        (now - p.LastActiveUtc) <= InactivityTimeout)
            .ToList();
    }

    public void RemovePlayback(string userId)
    {
        _activePlaybacks.TryRemove(userId, out _);
    }

    private void PruneExpired()
    {
        var now = DateTime.UtcNow;
        foreach (var kvp in _activePlaybacks)
        {
            if ((now - kvp.Value.LastActiveUtc) > InactivityTimeout)
            {
                _activePlaybacks.TryRemove(kvp.Key, out _);
            }
        }
    }
}