using System.Windows;
using DrivingAcademy.Data;
using DrivingAcademy.ViewModels;
using MySqlConnector;

namespace DrivingAcademy;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= Window_Loaded;

        try
        {
            using var database = new AcademyContext();
            var tracks = await database.LoadTracksAsync();
            var model = new LeaderboardViewModel(
                tracks,
                await database.LoadConfigurationsAsync(),
                await database.LoadCarsAsync()
            );

            DataContext = model;
            await model.ShowResultsAsync();

            if (tracks.Length == 0)
                MessageBox.Show(this, "Справочник трасс пуст.", "Driving Academy");
        }
        catch (Exception error)
            when (error is MySqlException or ArgumentException or InvalidOperationException)
        {
            string message = error is MySqlException mysql
                ? $"Не удалось прочитать трассы из MySQL (код {mysql.Number}). Проверьте доступ к базе данных"
                : "Проверьте ConnectionString в файле Data/AcademyContext.cs.";
            MessageBox.Show(this, message, "Подключение к базе");
        }
    }

    private PilotSession? session;
    private int profileRequest;
    private bool registering;
    private readonly AccountViewModel account = new();

    private void ShowLeaderboard()
    {
        Secret.Clear();
        Confirmation.Clear();

        Auth.Visibility = Visibility.Collapsed;
        ResultsPanel.Visibility = ResultEditor.Visibility = Visibility.Collapsed;
        ResultsPanel.DataContext = null;

        ++profileRequest;
        Profile.DataContext = null;
        Profile.Visibility = Visibility.Collapsed;

        BoardTab.BorderThickness = new Thickness(0, 0, 0, 2);
        ProfileTab.BorderThickness = new Thickness(0);
        Board.Visibility = Visibility.Visible;
    }

    private void OpenForm(bool registration)
    {
        registering = registration;
        Board.Visibility = Visibility.Collapsed;
        Auth.Visibility = Visibility.Visible;
        Auth.DataContext = account;
        Names.Visibility = Repeat.Visibility = registration
            ? Visibility.Visible
            : Visibility.Collapsed;

        AuthSubtitle.Text = registration
            ? "Сохраняйте историю своих результатов."
            : "Ваши результаты и история кругов.";
        AuthTitle.Text = registration ? "Создать аккаунт" : "Войти в аккаунт";
        Submit.Content = registration ? "Зарегистрироваться" : "Войти";
        SwitchForm.Content = registration ? "Уже есть аккаунт? Войти" : "Нет аккаунта? Регистрация";

        Feedback.Text = "";
        Secret.Clear();
        Confirmation.Clear();
        LoginField.Focus();
    }

    private async void Profile_Click(object sender, RoutedEventArgs e)
    {
        if (session is not { } pilot)
            return;

        ResultsPanel.Visibility = ResultEditor.Visibility = Visibility.Collapsed;
        ResultsPanel.DataContext = null;
        Board.Visibility = Auth.Visibility = Visibility.Collapsed;
        Profile.Visibility = Visibility.Visible;
        BoardTab.BorderThickness = new Thickness(0);
        ProfileTab.BorderThickness = new Thickness(0, 0, 0, 2);

        Profile.DataContext = null;
        HistoryStatus.Text = "Загрузка истории…";
        var request = ++profileRequest;

        try
        {
            var model = await ProfileViewModel.LoadAsync(pilot.Id);
            if (request != profileRequest)
                return;

            Profile.DataContext = model;
            HistoryStatus.Text = model.Laps.Length == 0 ? "У вас пока нет заездов." : "";
        }
        catch (Exception error)
            when (error is MySqlException or InvalidOperationException or ArgumentException)
        {
            if (request == profileRequest)
                HistoryStatus.Text =
                    "Не удалось загрузить профиль. Откройте личный кабинет ещё раз.";
        }
    }

    private void Switch_Click(object sender, RoutedEventArgs e) => OpenForm(!registering);

    private void Board_Click(object sender, RoutedEventArgs e) => ShowLeaderboard();

    private void Account_Click(object sender, RoutedEventArgs e)
    {
        if (session == null)
        {
            OpenForm(false);
            return;
        }

        session = null;
        UsersTab.Visibility = UsersPanel.Visibility = Visibility.Collapsed;
        UsersPanel.DataContext = null;
        editingUser = null;
        ManageTab.Visibility = Visibility.Collapsed;
        ProfileTab.Visibility = Visibility.Collapsed;

        Identity.Text = "";
        AccountButton.Content = "Войти";
        ShowLeaderboard();
    }

    private async void Submit_Click(object sender, RoutedEventArgs e)
    {
        LayoutRoot.IsEnabled = false;
        try
        {
            if (registering)
            {
                await account.RegisterAsync(Secret.Password, Confirmation.Password);
                OpenForm(false);
                Feedback.Text = "Аккаунт создан. Введите пароль для входа.";
            }
            else
            {
                session = await account.LoginAsync(Secret.Password);

                Identity.Text = $"{session.DisplayName}\n{session.Role}";
                AccountButton.Content = "Выйти";
                ProfileTab.Visibility = Visibility.Visible;
                UsersTab.Visibility =
                    session.RoleId == 3 ? Visibility.Visible : Visibility.Collapsed;
                ManageTab.Visibility = session.RoleId is 2 or 3
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                ShowLeaderboard();
            }
        }
        catch (ArgumentException error)
        {
            Feedback.Text = error.Message;
        }
        catch (Exception error)
            when (error is MySqlException or InvalidOperationException or FormatException)
        {
            Feedback.Text = "Не удалось выполнить запрос. Проверьте подключение и схему базы.";
        }
        finally
        {
            LayoutRoot.IsEnabled = true;
        }
    }
}
