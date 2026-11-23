using DrivingAcademy.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DrivingAcademy.ViewModels;

internal record Choice(uint Id, string Label);
internal record ResultEntry(uint Id, uint PilotId, uint CircuitId, uint CarId, string Pilot, string Circuit,
    string Car, uint Milliseconds, DateTime DrivenAt)
{
    public string Time => $"{Milliseconds / 60000}:{Milliseconds / 1000 % 60:00}.{Milliseconds % 1000:000}";
}

internal class ResultsViewModel
{
    public Choice[] Pilots { get; private set; } = [];
    public CircuitEntry[] Layouts { get; private set; } = [];
    public Choice[] Cars { get; private set; } = [];
    public ResultEntry[] Entries { get; private set; } = [];

    public ResultsViewModel(PilotSession session)
    {
        if (session.RoleId is not (2 or 3)) throw new UnauthorizedAccessException();
    }

    public async Task LoadAsync()
    {
        using var db = new AcademyContext();
        Pilots = await db.Database.SqlQuery<Choice>($"SELECT id, CONCAT(name,' ',surname,' (',username,')') AS Label FROM users ORDER BY surname,name,username").ToArrayAsync();
        Layouts = await db.Database.SqlQuery<CircuitEntry>($"SELECT cfg.id,cfg.track_id AS TrackId,t.name AS Track,cfg.name AS Name FROM track_configurations cfg JOIN tracks t ON cfg.track_id=t.id ORDER BY t.name,cfg.name").ToArrayAsync();
        Cars = await db.Database.SqlQuery<Choice>($"SELECT id,name AS Label FROM cars ORDER BY name").ToArrayAsync();
        Entries = await db.Database.SqlQuery<ResultEntry>($"""
            SELECT l.id, l.user_id AS PilotId, l.track_configuration_id AS CircuitId, l.car_id AS CarId,
                   CONCAT(u.name,' ',u.surname) AS Pilot, CONCAT(t.name,' · ',cfg.name) AS Circuit,
                   c.name AS Car, l.lap_time_ms AS Milliseconds, l.driven_at AS DrivenAt
            FROM lap_results l JOIN users u ON u.id=l.user_id JOIN cars c ON c.id=l.car_id
            JOIN track_configurations cfg ON cfg.id=l.track_configuration_id JOIN tracks t ON t.id=cfg.track_id
            ORDER BY l.driven_at DESC,l.id DESC
            """).ToArrayAsync();
    }

    public async Task SaveAsync(ResultEntry? original, Choice? pilot, Choice? circuit, Choice? car, string time, string date)
    {
        if (pilot == null || circuit == null || car == null)
            throw new ArgumentException("Выберите пилота, конфигурацию и автомобиль.");
        var match = Regex.Match(time.Trim(), @"^([0-9]{1,5}):([0-5][0-9])\.([0-9]{3})$");
        if (!match.Success) throw new ArgumentException("Введите время круга в формате 1:31.300.");
        long ms = long.Parse(match.Groups[1].Value) * 60000 + int.Parse(match.Groups[2].Value) * 1000 + int.Parse(match.Groups[3].Value);
        if (ms is <= 0 or > uint.MaxValue) throw new ArgumentException("Время круга вне допустимого диапазона.");
        if (!DateTime.TryParseExact(date.Trim(), "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var driven) || driven.Year < 1000)
            throw new ArgumentException("Введите дату и время в формате дд.ММ.гггг ЧЧ:мм:сс.");
        using var db = new AcademyContext();
        if (original == null)
        {
            await db.Database.ExecuteSqlAsync($"INSERT INTO lap_results(user_id,track_configuration_id,car_id,lap_time_ms,driven_at) VALUES({pilot.Id},{circuit.Id},{car.Id},{ms},{driven})");
            return;
        }
        var count = await db.Database.ExecuteSqlAsync($"""
            UPDATE lap_results SET user_id={pilot.Id},track_configuration_id={circuit.Id},car_id={car.Id},lap_time_ms={ms},driven_at={driven}
            WHERE id={original.Id} AND user_id={original.PilotId} AND track_configuration_id={original.CircuitId}
            AND car_id={original.CarId} AND lap_time_ms={original.Milliseconds} AND driven_at={original.DrivenAt}
            """);
        CheckAffected(count);
    }

    public async Task DeleteAsync(ResultEntry entry)
    {
        using var db = new AcademyContext();
        var count = await db.Database.ExecuteSqlAsync($"""
            DELETE FROM lap_results WHERE id={entry.Id} AND user_id={entry.PilotId} AND track_configuration_id={entry.CircuitId}
            AND car_id={entry.CarId} AND lap_time_ms={entry.Milliseconds} AND driven_at={entry.DrivenAt}
            """);
        CheckAffected(count);
    }

    private static void CheckAffected(int count)
    {
        if (count == 0) throw new ArgumentException("Запись изменилась. Откройте раздел заново и проверьте результат.");
    }
}

