using System.Windows;
using DrivingAcademy.ViewModels;

namespace DrivingAcademy;

public partial class MainWindow
{
    private UserEntry? editingUser;

    private async void Users_Click(object sender, RoutedEventArgs e)
    {
        if (session?.RoleId != 3)
            return;

        ResultTools.Visibility =
            TrackTools.Visibility =
            ConfigTools.Visibility =
            CarTools.Visibility =
                Visibility.Collapsed;

        UsersPanel.Visibility = Visibility.Visible;
        UserEditor.Visibility = Visibility.Collapsed;
        UsersPanel.DataContext = null;
        editingUser = null;

        await RunResultsAsync(async () =>
        {
            var model = new UsersViewModel(session);
            await model.LoadAsync();
            UsersPanel.DataContext = model;
        });
    }

    private void EditUser(object sender, RoutedEventArgs e)
    {
        if (
            ((sender as FrameworkElement)?.DataContext ?? UsersGrid.SelectedItem)
            is not UserEntry user
        )
        {
            ResultStatus.Text = "Выберите пользователя.";
            return;
        }

        editingUser = user;
        UserEditorTitle.Text = $"Изменить пользователя · {user.Login}";
        UserFirstName.Text = user.FirstName;
        UserSurname.Text = user.Surname;
        UserRoleBox.SelectedValue = user.RoleId;

        UserEditor.Visibility = Visibility.Visible;
        ResultStatus.Text = "";
    }

    private void CancelUser(object sender, RoutedEventArgs e) =>
        UserEditor.Visibility = Visibility.Collapsed;

    private async void SaveUser(object sender, RoutedEventArgs e)
    {
        if (
            session?.RoleId != 3
            || editingUser is not { } original
            || UsersPanel.DataContext is not UsersViewModel model
        )
            return;

        var role = UserRoleBox.SelectedItem as UserRole;

        await RunResultsAsync(async () =>
        {
            await model.SaveAsync(original, UserFirstName.Text, UserSurname.Text, role);

            if (session.Id == original.Id)
            {
                session = session with
                {
                    DisplayName = $"{UserFirstName.Text.Trim()} {UserSurname.Text.Trim()}",
                    RoleId = role!.Id,
                    Role = role.Name,
                };
                Identity.Text = $"{session.DisplayName}\n{session.Role}";

                if (role.Id != 3)
                {
                    UsersTab.Visibility = Visibility.Collapsed;
                    ManageTab.Visibility = role.Id == 2 ? Visibility.Visible : Visibility.Collapsed;
                    UsersPanel.DataContext = null;
                    ShowLeaderboard();
                    return;
                }
            }
            UserEditor.Visibility = Visibility.Collapsed;

            await model.LoadAsync();

            UsersPanel.DataContext = null;
            UsersPanel.DataContext = model;
            ResultStatus.Text = "Данные пользователя сохранены.";
        });
    }
}
