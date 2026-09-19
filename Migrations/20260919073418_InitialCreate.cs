using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpoMusic.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Track",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    spotifyId = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    artists = table.Column<List<string>>(type: "text[]", nullable: false),
                    album = table.Column<string>(type: "text", nullable: false),
                    durationMs = table.Column<int>(type: "integer", nullable: false),
                    popularity = table.Column<int>(type: "integer", nullable: false),
                    url = table.Column<string>(type: "text", nullable: false),
                    imageUrl = table.Column<string>(type: "text", nullable: true),
                    previewUrl = table.Column<string>(type: "text", nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Track", x => x.id);
                    table.UniqueConstraint("AK_Track_spotifyId", x => x.spotifyId);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    spotifyId = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: false),
                    image = table.Column<string>(type: "text", nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ListeningHistory",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    userId = table.Column<string>(type: "text", nullable: false),
                    trackId = table.Column<string>(type: "text", nullable: false),
                    playedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListeningHistory", x => x.id);
                    table.ForeignKey(
                        name: "FK_ListeningHistory_Track_trackId",
                        column: x => x.trackId,
                        principalTable: "Track",
                        principalColumn: "spotifyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ListeningHistory_User_userId",
                        column: x => x.userId,
                        principalTable: "User",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Match",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user1Id = table.Column<string>(type: "text", nullable: false),
                    user2Id = table.Column<string>(type: "text", nullable: false),
                    matchScore = table.Column<double>(type: "double precision", nullable: false),
                    commonTracks = table.Column<int>(type: "integer", nullable: false),
                    commonArtists = table.Column<int>(type: "integer", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Match", x => x.id);
                    table.ForeignKey(
                        name: "FK_Match_User_user1Id",
                        column: x => x.user1Id,
                        principalTable: "User",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Match_User_user2Id",
                        column: x => x.user2Id,
                        principalTable: "User",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpotifyToken",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    userId = table.Column<string>(type: "text", nullable: false),
                    accessToken = table.Column<string>(type: "text", nullable: false),
                    refreshToken = table.Column<string>(type: "text", nullable: false),
                    expiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expiresIn = table.Column<int>(type: "integer", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpotifyToken", x => x.id);
                    table.ForeignKey(
                        name: "FK_SpotifyToken_User_userId",
                        column: x => x.userId,
                        principalTable: "User",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTopTrack",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    userId = table.Column<string>(type: "text", nullable: false),
                    trackId = table.Column<string>(type: "text", nullable: false),
                    rank = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<double>(type: "double precision", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTopTrack", x => x.id);
                    table.ForeignKey(
                        name: "FK_UserTopTrack_Track_trackId",
                        column: x => x.trackId,
                        principalTable: "Track",
                        principalColumn: "spotifyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTopTrack_User_userId",
                        column: x => x.userId,
                        principalTable: "User",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ListeningHistory_playedAt",
                table: "ListeningHistory",
                column: "playedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ListeningHistory_trackId",
                table: "ListeningHistory",
                column: "trackId");

            migrationBuilder.CreateIndex(
                name: "IX_ListeningHistory_userId",
                table: "ListeningHistory",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_ListeningHistory_userId_trackId_playedAt",
                table: "ListeningHistory",
                columns: new[] { "userId", "trackId", "playedAt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Match_user1Id",
                table: "Match",
                column: "user1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Match_user1Id_user2Id",
                table: "Match",
                columns: new[] { "user1Id", "user2Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Match_user2Id",
                table: "Match",
                column: "user2Id");

            migrationBuilder.CreateIndex(
                name: "IX_SpotifyToken_userId",
                table: "SpotifyToken",
                column: "userId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Track_spotifyId",
                table: "Track",
                column: "spotifyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_email",
                table: "User",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_spotifyId",
                table: "User",
                column: "spotifyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTopTrack_trackId",
                table: "UserTopTrack",
                column: "trackId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTopTrack_userId",
                table: "UserTopTrack",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTopTrack_userId_trackId",
                table: "UserTopTrack",
                columns: new[] { "userId", "trackId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListeningHistory");

            migrationBuilder.DropTable(
                name: "Match");

            migrationBuilder.DropTable(
                name: "SpotifyToken");

            migrationBuilder.DropTable(
                name: "UserTopTrack");

            migrationBuilder.DropTable(
                name: "Track");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
