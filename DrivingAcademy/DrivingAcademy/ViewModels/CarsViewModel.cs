using DrivingAcademy.Data;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace DrivingAcademy.ViewModels;

internal sealed class CarsViewModel
{
    public CarsViewModel(PilotSession session)
    {
        if (session.RoleId is not (2 or 3)) throw new UnauthorizedAccessException();
    }

    public async Task<Choice[]> LoadAsync()
    {
        using var db = new AcademyContext();
        return await db.Database.SqlQuery<Choice>($"SELECT id, name AS Label FROM cars ORDER BY name").ToArrayAsync();
    }

    public async Task SaveAsync(Choice? original, string name)
    {
        name = name.Trim();
        if (name.Length is 0 or > 30) throw new ArgumentException("Введите название длиной от 1 до 30 символов.");
        using var db = new AcademyContext();
        try
        {
            if (original == null)
                await db.Database.ExecuteSqlAsync($"INSERT INTO cars(name) VALUES({name})");
            else
                CheckChanged(await db.Database.ExecuteSqlAsync($"UPDATE cars SET name={name} WHERE id={original.Id} AND BINARY name={original.Label}"));
        }
        catch (MySqlException error) when (error.Number == 1062)
        {
            throw new ArgumentException("Автомобиль с таким названием уже существует.");
        }
    }

    public async Task DeleteAsync(Choice original)
    {
        using var db = new AcademyContext();
        try
        {
            CheckChanged(await db.Database.ExecuteSqlAsync($"DELETE FROM cars WHERE id={original.Id} AND BINARY name={original.Label}"));
        }
        catch (MySqlException error) when (error.Number == 1451)
        {
            throw new ArgumentException("На этом автомобиле есть заезды. Удаление невозможно.");
        }
    }

    private static void CheckChanged(int count)
    {
        if (count == 0) throw new ArgumentException("Автомобиль уже изменили или удалили. Откройте справочник заново.");
    }
}

