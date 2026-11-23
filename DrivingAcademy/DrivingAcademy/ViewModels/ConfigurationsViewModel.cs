using DrivingAcademy.Data;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace DrivingAcademy.ViewModels;

internal record CircuitEntry(uint Id, uint TrackId, string Track, string Name);

internal sealed class ConfigurationsViewModel
{
    public ConfigurationsViewModel(PilotSession session)
    {
        if (session.RoleId is not (2 or 3)) throw new UnauthorizedAccessException();
    }

    public async Task<CircuitEntry[]> LoadAsync()
    {
        using var db = new AcademyContext();
        return await db.Database.SqlQuery<CircuitEntry>($"""
            SELECT cfg.id, cfg.track_id AS TrackId, t.name AS Track, cfg.name AS Name
            FROM track_configurations cfg JOIN tracks t ON t.id = cfg.track_id ORDER BY t.name, cfg.name
            """).ToArrayAsync();
    }

    public async Task SaveAsync(CircuitEntry? original, Choice? track, string name)
    {
        name = name.Trim();
        if (track == null) throw new ArgumentException("Выберите трассу.");
        if (name.Length is 0 or > 30) throw new ArgumentException("Введите название длиной от 1 до 30 символов.");
        using var db = new AcademyContext();
        try
        {
            if (original == null)
                await db.Database.ExecuteSqlAsync($"INSERT INTO track_configurations(track_id,name) VALUES({track.Id},{name})");
            else
                CheckChanged(await db.Database.ExecuteSqlAsync($"""
                    UPDATE track_configurations SET track_id={track.Id},name={name}
                    WHERE id={original.Id} AND track_id={original.TrackId} AND BINARY name={original.Name}
                    """));
        }
        catch (MySqlException error) when (error.Number == 1062)
        {
            throw new ArgumentException("На этой трассе уже есть конфигурация с таким названием.");
        }
    }

    public async Task DeleteAsync(CircuitEntry original)
    {
        using var db = new AcademyContext();
        try
        {
            CheckChanged(await db.Database.ExecuteSqlAsync($"""
                DELETE FROM track_configurations WHERE id={original.Id} AND track_id={original.TrackId} AND BINARY name={original.Name}
                """));
        }
        catch (MySqlException error) when (error.Number == 1451)
        {
            throw new ArgumentException("По этой конфигурации есть заезды. Удаление невозможно.");
        }
    }

    private static void CheckChanged(int count)
    {
        if (count == 0) throw new ArgumentException("Конфигурация изменилась. Откройте справочник заново.");
    }
}
