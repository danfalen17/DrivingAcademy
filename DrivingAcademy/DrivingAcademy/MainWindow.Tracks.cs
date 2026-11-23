using System.Windows;
using DrivingAcademy.Data;
using DrivingAcademy.ViewModels;

namespace DrivingAcademy;

public partial class MainWindow
{
    private Choice? editingTrack;

    private async void Tracks_Click(object sender, RoutedEventArgs e)
    {
        if (session?.RoleId is not (2 or 3))
            return;

        UsersPanel.Visibility = Visibility.Collapsed;
        CarTools.Visibility =
            ConfigTools.Visibility =
            ResultTools.Visibility =
                Visibility.Collapsed;

        TrackTools.Visibility = Visibility.Visible;
        TrackEditor.Visibility = Visibility.Collapsed;

        await RunResultsAsync(async () =>
        {
            var model = new TracksViewModel(session);
            TrackTools.DataContext = model;
            TrackGrid.ItemsSource = await model.LoadAsync();
        });
    }

    private void EditTrack(object sender, RoutedEventArgs e)
    {
        editingTrack =
            sender == AddTrack
                ? null
                : ((sender as FrameworkElement)?.DataContext ?? TrackGrid.SelectedItem) as Choice;

        if (sender != AddTrack && editingTrack == null)
        {
            ResultStatus.Text = "Выберите трассу.";
            return;
        }

        TrackName.Text = editingTrack?.Label ?? "";
        TrackEditorTitle.Text = editingTrack == null ? "Добавить запись" : "Изменить запись";

        TrackEditor.Visibility = Visibility.Visible;
        ResultStatus.Text = "";
        TrackName.Focus();
    }

    private void CancelTrack(object sender, RoutedEventArgs e) =>
        TrackEditor.Visibility = Visibility.Collapsed;

    private async Task RefreshTracksAsync(TracksViewModel model, string? oldName, string? newName)
    {
        TrackEditor.Visibility = Visibility.Collapsed;
        TrackGrid.ItemsSource = await model.LoadAsync();
        await RefreshBoardCatalogAsync(oldName, newName);
    }

    private async Task RefreshBoardCatalogAsync(
        string? oldName = null,
        string? newName = null,
        string? oldCar = null,
        string? newCar = null
    )
    {
        using var db = new AcademyContext();
        var previous = DataContext as LeaderboardViewModel;
        var board = new LeaderboardViewModel(
            await db.LoadTracksAsync(),
            await db.LoadConfigurationsAsync(),
            await db.LoadCarsAsync()
        );

        var selected = previous?.SelectedTrack;

        if (selected == oldName)
            selected = newName;

        if (selected != null && board.Tracks.Contains(selected))
            board.SelectedTrack = selected;

        if (previous?.SelectedConfiguration is string cfg && board.Configurations.Contains(cfg))
            board.SelectedConfiguration = cfg;

        var selectedCar = previous?.SelectedCar?.Name;

        if (oldCar != null && selectedCar == oldCar)
            selectedCar = newCar;

        board.SelectedCar = board.Cars.FirstOrDefault(c => c.Name == selectedCar) ?? board.Cars[0];
        DataContext = board;

        await board.ShowResultsAsync();
    }

    private async void SaveTrack(object sender, RoutedEventArgs e)
    {
        if (TrackTools.DataContext is not TracksViewModel model)
            return;

        await RunResultsAsync(async () =>
        {
            var name = TrackName.Text.Trim();
            await model.SaveAsync(editingTrack, name);
            await RefreshTracksAsync(model, editingTrack?.Label, name);
            ResultStatus.Text = "Трасса сохранена.";
        });
    }

    private async void DeleteTrack(object sender, RoutedEventArgs e)
    {
        if (TrackTools.DataContext is not TracksViewModel model)
            return;

        if (
            ((sender as FrameworkElement)?.DataContext ?? TrackGrid.SelectedItem)
            is not Choice track
        )
        {
            ResultStatus.Text = "Выберите трассу.";
            return;
        }

        if (
            MessageBox.Show(
                this,
                $"Удалить трассу «{track.Label}»?",
                "Удаление трассы",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No
            ) != MessageBoxResult.Yes
        )
            return;

        await RunResultsAsync(async () =>
        {
            await model.DeleteAsync(track);
            await RefreshTracksAsync(model, track.Label, null);
            ResultStatus.Text = "Трасса удалена.";
        });
    }
}
