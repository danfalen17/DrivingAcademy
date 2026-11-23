using System.Security.Cryptography;
using System.Text;
using DrivingAcademy.Data;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace DrivingAcademy.ViewModels;

internal record PilotSession(uint Id, string DisplayName, byte RoleId, string Role);

internal record LoginRecord(
    uint Id,
    string DisplayName,
    byte RoleId,
    string Role,
    string StoredPassword
);

internal sealed class AccountViewModel
{
    public string Username { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string Surname { get; set; } = "";

    public async Task RegisterAsync(string password, string confirmation)
    {
        if (
            string.IsNullOrWhiteSpace(Username)
            || string.IsNullOrWhiteSpace(FirstName)
            || string.IsNullOrWhiteSpace(Surname)
            || string.IsNullOrWhiteSpace(password)
        )
            throw new ArgumentException("Заполните все поля.");
        if (password != confirmation)
            throw new ArgumentException("Пароли не совпадают.");

        var hash = await Task.Run(() => Hash(password));
        using var db = new AcademyContext();
        try
        {
            await db.Database.ExecuteSqlAsync(
                $"""
                INSERT INTO users (username, name, surname, password, role_id)
                VALUES ({Username.Trim()}, {FirstName.Trim()}, {Surname.Trim()}, {hash}, 1)
                """
            );
        }
        catch (MySqlException error) when (error.Number == 1062)
        {
            throw new ArgumentException("Этот логин уже занят.");
        }
    }

    public async Task<PilotSession> LoginAsync(string password)
    {
        if (string.IsNullOrWhiteSpace(Username) || password.Length == 0)
            throw new ArgumentException("Введите логин и пароль.");

        using var db = new AcademyContext();
        var user = await db
            .Database.SqlQuery<LoginRecord>(
                $"""
                SELECT u.id AS Id, CONCAT(u.name,' ',u.surname) AS DisplayName,
                       u.role_id AS RoleId, r.name AS Role, u.password AS StoredPassword
                FROM users u JOIN roles r ON r.id = u.role_id WHERE u.username = {Username.Trim()}
                """
            )
            .SingleOrDefaultAsync();

        if (user == null)
            throw new ArgumentException("Неверный логин или пароль.");

        if (user.StoredPassword.Length <= 20)
        {
            if (user.StoredPassword != password)
                throw new ArgumentException("Неверный логин или пароль.");

            var hash = await Task.Run(() => Hash(password));
            await db.Database.ExecuteSqlAsync(
                $"UPDATE users SET password={hash} WHERE id={user.Id} AND BINARY password={user.StoredPassword}"
            );
        }
        else
        {
            var parts = user.StoredPassword.Split('$');
            if (parts.Length != 3 || parts[0] != "pbkdf2-v1")
                throw new ArgumentException("Неверный логин или пароль.");

            var actual = await Task.Run(() => Derive(password, Convert.FromBase64String(parts[1])));
            if (
                !CryptographicOperations.FixedTimeEquals(actual, Convert.FromBase64String(parts[2]))
            )
                throw new ArgumentException("Неверный логин или пароль.");
        }
        return new(user.Id, user.DisplayName, user.RoleId, user.Role);
    }

    private static byte[] Derive(string password, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            600000,
            HashAlgorithmName.SHA256,
            32
        );

    private static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        return $"pbkdf2-v1${Convert.ToBase64String(salt)}${Convert.ToBase64String(Derive(password, salt))}";
    }
}
