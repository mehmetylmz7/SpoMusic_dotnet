namespace SpoMusic.Api.Services.Matching;

public record UserProfile(
    string UserId,
    string SpotifyId,
    List<string> TopTracks,
    List<string> TopArtists,
    List<ListeningHistoryItem> ListeningHistory
);

public record ListeningHistoryItem(
    string TrackId,
    DateTime PlayedAt
);

public record MatchScoreResult(
    int CommonTracks,
    int CommonArtists,
    double RecencyScore,
    double TopSimilarity,
    double Total
);

public static class MatchingEngine
{
    public static MatchScoreResult CalculateScore(UserProfile profileA, UserProfile profileB)
    {
        int commonTracks = CountCommonTracks(profileA, profileB);
        int commonArtists = CountCommonArtists(profileA, profileB);
        double recencyScore = CalculateRecencyScore(profileA, profileB);
        double topSimilarity = CalculateTopSimilarity(profileA, profileB, commonTracks, commonArtists);

        int maxTracks = Math.Max(Math.Max(profileA.TopTracks.Count, profileB.TopTracks.Count), 1);
        int maxArtists = Math.Max(Math.Max(profileA.TopArtists.Count, profileB.TopArtists.Count), 1);

        double trackRatio = Math.Min(1.0, (double)commonTracks / maxTracks);
        double artistRatio = Math.Min(1.0, (double)commonArtists / maxArtists);

        double total = Math.Min(
            1.0,
            Math.Max(
                0.0,
                trackRatio * 0.40 +
                artistRatio * 0.25 +
                recencyScore * 0.20 +
                topSimilarity * 0.15
            )
        );

        return new MatchScoreResult(
            commonTracks,
            commonArtists,
            Math.Round(recencyScore, 4),
            Math.Round(topSimilarity, 4),
            Math.Round(total, 4)
        );
    }

    private static int CountCommonTracks(UserProfile profileA, UserProfile profileB)
    {
        var setB = new HashSet<string>(profileB.TopTracks, StringComparer.OrdinalIgnoreCase);
        return profileA.TopTracks.Count(t => setB.Contains(t));
    }

    private static int CountCommonArtists(UserProfile profileA, UserProfile profileB)
    {
        var setB = new HashSet<string>(profileB.TopArtists, StringComparer.OrdinalIgnoreCase);
        return profileA.TopArtists.Count(a => setB.Contains(a));
    }

    private static double CalculateRecencyScore(UserProfile profileA, UserProfile profileB)
    {
        var now = DateTime.UtcNow;
        var thirtyDays = TimeSpan.FromDays(30);

        var recentA = profileA.ListeningHistory
            .Where(h => (now - h.PlayedAt) < thirtyDays)
            .OrderByDescending(h => h.PlayedAt)
            .ToList();

        var recentB = profileB.ListeningHistory
            .Where(h => (now - h.PlayedAt) < thirtyDays)
            .OrderByDescending(h => h.PlayedAt)
            .ToList();

        if (recentA.Count == 0 || recentB.Count == 0)
        {
            return 0.5;
        }

        int recentCount = Math.Min(Math.Min(recentA.Count, recentB.Count), 10);
        double weightedScore = 0.0;

        for (int i = 0; i < recentCount; i++)
        {
            double daysA = Math.Max(0.0, (now - recentA[i].PlayedAt).TotalDays);
            double daysB = Math.Max(0.0, (now - recentB[i].PlayedAt).TotalDays);

            double recencyWeightA = 1.0 / (1.0 + daysA);
            double recencyWeightB = 1.0 / (1.0 + daysB);

            if (string.Equals(recentA[i].TrackId, recentB[i].TrackId, StringComparison.OrdinalIgnoreCase))
            {
                weightedScore += (recencyWeightA + recencyWeightB) / 2.0;
            }
        }

        return Math.Min(1.0, weightedScore / recentCount);
    }

    private static double CalculateTopSimilarity(UserProfile profileA, UserProfile profileB, int commonTracks, int commonArtists)
    {
        int maxTracks = Math.Max(Math.Max(profileA.TopTracks.Count, profileB.TopTracks.Count), 1);
        int maxArtists = Math.Max(Math.Max(profileA.TopArtists.Count, profileB.TopArtists.Count), 1);

        double trackSimilarity = (double)commonTracks / maxTracks;
        double artistSimilarity = (double)commonArtists / maxArtists;

        return (trackSimilarity + artistSimilarity) / 2.0;
    }
}
