using DrivingAcademy.Data;
using Microsoft.EntityFrameworkCore;

namespace DrivingAcademy.ViewModels;

internal record UserEntry(
    uint Id,
    string Login,
    string FirstName,
    string Surname,
    byte RoleId,
    string Role
);

internal record UserRole(byte Id, string Name);

internal sealed class UsersViewModel
{
    private readonly uint administratorId;
    public UserEntry[] Users { get; private set; } = [];
    public UserRole[] Roles { get; private set; } = [];

    public UsersViewModel(PilotSession session)
    {
        if (session.RoleId != 3)
            throw new ArgumentException("Раздел доступен системному администратору.");
        administratorId = session.Id;
    }

    private async Task CheckAccessAsync(AcademyContext db)
    {
        var roles = await db
            .Database.SqlQuery<byte>(
                $"SELECT role_id FROM users WHERE id={administratorId} FOR UPDATE"
            )
            .ToArrayAsync();
        if (roles.Length != 1 || roles[0] != 3)
            throw new ArgumentException(
                "Доступ к управлению пользователями закрыт. Войдите заново."
            );
    }

    public async Task LoadAsync()
    {
        using var db = new AcademyContext();
        await using var transaction = await db.Database.BeginTransactionAsync();
        await CheckAccessAsync(db);
        Roles = await db
            .Database.SqlQuery<UserRole>($"SELECT id, name FROM roles ORDER BY id")
            .ToArrayAsync();
        Users = await db
            .Database.SqlQuery<UserEntry>(
                $"""
                SELECT u.id, u.username AS Login, u.name AS FirstName, u.surname AS Surname,
                       u.role_id AS RoleId, r.name AS Role
                FROM users u JOIN roles r ON r.id=u.role_id ORDER BY u.surname,u.name,u.username
                """
            )
            .ToArrayAsync();

        await transaction.CommitAsync();
    }

    public async Task SaveAsync(
        UserEntry original,
        string firstName,
        string surname,
        UserRole? role
    )
    {
        firstName = firstName.Trim();
        surname = surname.Trim();
        if (firstName.Length is 0 or > 20 || surname.Length is 0 or > 100 || role == null)
            throw new ArgumentException("Введите имя, фамилию и выберите роль.");

        using var db = new AcademyContext();
        await using var transaction = await db.Database.BeginTransactionAsync();
        await CheckAccessAsync(db);

        var changed = await db.Database.ExecuteSqlAsync(
            $"""
            UPDATE users SET name={firstName},surname={surname},role_id={role.Id}
            WHERE id={original.Id} AND BINARY name={original.FirstName} AND BINARY surname={original.Surname} AND role_id={original.RoleId}
            """
        );

        if (changed == 0)
            throw new ArgumentException("Данные пользователя изменились. Откройте раздел заново.");
        await transaction.CommitAsync();
    }
}
