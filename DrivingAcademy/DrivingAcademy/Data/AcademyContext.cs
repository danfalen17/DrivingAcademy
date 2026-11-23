using Microsoft.EntityFrameworkCore;

namespace DrivingAcademy.Data;

internal sealed class AcademyContext : DbContext
{
    private const string ConnectionString =
        "Server=localhost;Port=3306;Database=driving_academy;User ID=root;Password=1702;";

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseMySql(ConnectionString, new MySqlServerVersion(new Version(8, 0)));
    }

    public Task<string[]> LoadTracksAsync() =>
        Database.SqlQuery<string>($"SELECT name FROM tracks ORDER BY name").ToArrayAsync();

    public Task<string[]> LoadCarsAsync() =>
        Database.SqlQuery<string>($"SELECT name FROM cars ORDER BY name").ToArrayAsync();

    public Task<TrackConfiguration[]> LoadConfigurationsAsync() =>
        Database
            .SqlQuery<TrackConfiguration>(
                $"""
                SELECT t.name AS Track, cfg.name AS Name FROM track_configurations cfg
                JOIN tracks t ON t.id = cfg.track_id ORDER BY t.name, cfg.name
                """
            )
            .ToArrayAsync();

    public Task<RecordLap[]> LoadRecordsAsync(string track, string configuration, string? car) =>
        Database
            .SqlQuery<RecordLap>(
                $"""
                SELECT CONCAT(u.name, ' ', u.surname) AS Pilot, r.lap_time_ms AS Milliseconds, r.driven_at AS DrivenAt
                FROM (
                    SELECT l.*, ROW_NUMBER() OVER (
                        PARTITION BY l.user_id ORDER BY l.lap_time_ms, l.driven_at, l.id
                    ) AS position_in_history
                    FROM lap_results l
                    JOIN track_configurations cfg ON cfg.id = l.track_configuration_id
                    JOIN tracks t ON t.id = cfg.track_id
                    JOIN cars c ON c.id = l.car_id
                    WHERE t.name = {track} AND cfg.name = {configuration} AND ({car
                    == null} OR c.name = {car ?? string.Empty})
                ) r
                JOIN users u ON u.id = r.user_id
                WHERE r.position_in_history = 1
                ORDER BY r.lap_time_ms, r.driven_at, r.user_id
                """
            )
            .ToArrayAsync();
}

internal record TrackConfiguration(string Track, string Name);

internal record RecordLap(string Pilot, uint Milliseconds, DateTime DrivenAt);
