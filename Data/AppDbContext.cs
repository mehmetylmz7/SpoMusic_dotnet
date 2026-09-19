using Microsoft.EntityFrameworkCore;
using SpoMusic.Api.Entities;

namespace SpoMusic.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<SpotifyToken> SpotifyTokens => Set<SpotifyToken>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<ListeningHistory> ListeningHistories => Set<ListeningHistory>();
    public DbSet<UserTopTrack> UserTopTracks => Set<UserTopTrack>();
    public DbSet<Match> Matches => Set<Match>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.SpotifyId).IsUnique();
        });

        // SpotifyToken
        modelBuilder.Entity<SpotifyToken>(entity =>
        {
            entity.ToTable("SpotifyToken");
            entity.HasIndex(st => st.UserId).IsUnique();
        });

        // Track
        modelBuilder.Entity<Track>(entity =>
        {
            entity.ToTable("Track");
            entity.HasIndex(t => t.SpotifyId).IsUnique();
        });

        // ListeningHistory
        modelBuilder.Entity<ListeningHistory>(entity =>
        {
            entity.ToTable("ListeningHistory");
            entity.HasIndex(lh => new { lh.UserId, lh.TrackId, lh.PlayedAt }).IsUnique();
            entity.HasIndex(lh => lh.UserId);
            entity.HasIndex(lh => lh.PlayedAt);

            entity.HasOne(lh => lh.Track)
                .WithMany(t => t.ListeningHistories)
                .HasPrincipalKey(t => t.SpotifyId)
                .HasForeignKey(lh => lh.TrackId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserTopTrack
        modelBuilder.Entity<UserTopTrack>(entity =>
        {
            entity.ToTable("UserTopTrack");
            entity.HasIndex(ut => new { ut.UserId, ut.TrackId }).IsUnique();
            entity.HasIndex(ut => ut.UserId);

            entity.HasOne(ut => ut.Track)
                .WithMany(t => t.UserTopTracks)
                .HasPrincipalKey(t => t.SpotifyId)
                .HasForeignKey(ut => ut.TrackId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Match
        modelBuilder.Entity<Match>(entity =>
        {
            entity.ToTable("Match");
            entity.HasIndex(m => new { m.User1Id, m.User2Id }).IsUnique();
            entity.HasIndex(m => m.User1Id);
            entity.HasIndex(m => m.User2Id);

            entity.HasOne(m => m.User1)
                .WithMany(u => u.MatchesInitiated)
                .HasForeignKey(m => m.User1Id)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.User2)
                .WithMany(u => u.MatchesReceived)
                .HasForeignKey(m => m.User2Id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Convert all property column names to camelCase to match PostgreSQL Prisma schema
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                var name = property.Name;
                if (!string.IsNullOrEmpty(name))
                {
                    var camelCase = char.ToLowerInvariant(name[0]) + name[1..];
                    property.SetColumnName(camelCase);
                }
            }
        }
    }
}
