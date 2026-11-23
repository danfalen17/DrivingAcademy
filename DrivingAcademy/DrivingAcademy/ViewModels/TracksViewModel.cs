using DrivingAcademy.Data;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace DrivingAcademy.ViewModels;

internal sealed class TracksViewModel
{
    public TracksViewModel(PilotSession session)
    {
        if (session.RoleId is not (2 or 3))
            throw new UnauthorizedAccessException();
    }

    public async Task<Choice[]> LoadAsync()
    {
        using var db = new AcademyContext();
        return await db
            .Database.SqlQuery<Choice>($"SELECT id, name AS Label FROM tracks ORDER BY name")
            .ToArrayAsync();
    }

    public async Task SaveAsync(Choice? original, string name)
    {
        name = name.Trim();
        if (name.Length is 0 or > 30)
            throw new ArgumentException("Введите название длиной от 1 до 30 символов.");

        using var db = new AcademyContext();
        try
        {
            if (original == null)
                await db.Database.ExecuteSqlAsync($"INSERT INTO tracks(name) VALUES({name})");
            else
                CheckChanged(
                    await db.Database.ExecuteSqlAsync(
                        $"UPDATE tracks SET name={name} WHERE id={original.Id} AND BINARY name={original.Label}"
                    )
                );
        }
        catch (MySqlException error) when (error.Number == 1062)
        {
            throw new ArgumentException("Трасса с таким названием уже существует.");
        }
    }

    public async Task DeleteAsync(Choice original)
    {
        using var db = new AcademyContext();
        try
        {
            CheckChanged(
                await db.Database.ExecuteSqlAsync(
                    $"DELETE FROM tracks WHERE id={original.Id} AND BINARY name={original.Label}"
                )
            );
        }
        catch (MySqlException error) when (error.Number == 1451)
        {
            throw new ArgumentException("У трассы есть конфигурации. Удаление невозможно.");
        }
    }

    private static void CheckChanged(int count)
    {
        if (count == 0)
            throw new ArgumentException(
                "Трассу уже изменили или удалили. Откройте справочник заново."
            );
    }
}
