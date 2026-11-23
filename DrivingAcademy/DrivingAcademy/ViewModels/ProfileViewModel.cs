using DrivingAcademy.Data;
using Microsoft.EntityFrameworkCore;

namespace DrivingAcademy.ViewModels;

internal record PilotDetails(string Name, string Surname, string Username, DateTime CreatedAt, string Role)
{
    public string Initials => $"{Name.FirstOrDefault()}{Surname.FirstOrDefault()}".Trim('\0');
    public string FullName => $"{Name} {Surname}";
}

internal record HistoryLap(string Track, string Configuration, string Car, uint Milliseconds, DateTime DrivenAt)
{
    public string Circuit => $"{Track} · {Configuration}";
    public string LapTime => $"{Milliseconds / 60000}:{Milliseconds / 1000 % 60:00}.{Milliseconds % 1000:000}";
}

internal record ProfileViewModel(PilotDetails Pilot, HistoryLap[] Laps)
{
    public static async Task<ProfileViewModel> LoadAsync(uint userId)
    {
        using var db = new AcademyContext();
        var pilot = await db.Database.SqlQuery<PilotDetails>($"""
            SELECT u.name AS Name, u.surname AS Surname, u.username AS Username,
                   u.created_at AS CreatedAt, r.name AS Role
            FROM users u JOIN roles r ON r.id = u.role_id WHERE u.id = {userId}
            """).SingleAsync();
        var laps = await db.Database.SqlQuery<HistoryLap>($"""
            SELECT t.name AS Track, cfg.name AS Configuration, c.name AS Car,
                   l.lap_time_ms AS Milliseconds, l.driven_at AS DrivenAt
            FROM lap_results l
            JOIN track_configurations cfg ON cfg.id = l.track_configuration_id
            JOIN tracks t ON t.id = cfg.track_id
            JOIN cars c ON c.id = l.car_id
            WHERE l.user_id = {userId}
            ORDER BY l.driven_at DESC, l.id DESC
            """).ToArrayAsync();
        return new(pilot, laps);
    }
}
