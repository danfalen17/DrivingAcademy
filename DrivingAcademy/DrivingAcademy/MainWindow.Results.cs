using System.Windows;
using DrivingAcademy.ViewModels;
using MySqlConnector;

namespace DrivingAcademy;

public partial class MainWindow
{
    private ResultEntry? edited;

    private async void Results_Click(object sender, RoutedEventArgs e)
    {
        if (session?.RoleId is not (2 or 3)) return;
        ShowLeaderboard();
        Board.Visibility = Visibility.Collapsed;
        BoardTab.BorderThickness = new Thickness(0);
        UsersPanel.Visibility = Visibility.Collapsed;
        CarTools.Visibility = ConfigTools.Visibility = TrackTools.Visibility = TrackEditor.Visibility = Visibility.Collapsed;
        TrackTools.DataContext = null;
        ResultTools.Visibility = Visibility.Visible;
        ResultsPanel.Visibility = Visibility.Visible;
        await RunResultsAsync(async () =>
        {
            var model = new ResultsViewModel(session);
            await model.LoadAsync();
            ResultsPanel.DataContext = model;
            SetResultFilters(model);
        });
    }

    private async Task RunResultsAsync(Func<Task> action)
    {
        LayoutRoot.IsEnabled = false;
        ResultStatus.Text = "";
        try { await action(); }
        catch (ArgumentException error) { ResultStatus.Text = error.Message; }
        catch (Exception error) when (error is MySqlException or InvalidOperationException)
        { ResultStatus.Text = "Ошибка MySQL. Откройте раздел заново и проверьте состояние записи."; }
        finally { LayoutRoot.IsEnabled = true; }
    }

    private void SetResultFilters(ResultsViewModel model)
    {
        FilterTrack.ItemsSource = new[] { "Все трассы" }.Concat(model.Layouts.Select(l => l.Track).Distinct()).ToArray();
        FilterTrack.SelectedIndex = 0;
        FilterCar.ItemsSource = new[] { new Choice(0, "Все автомобили") }.Concat(model.Cars);
        FilterCar.SelectedIndex = 0;
        ResultsGrid.ItemsSource = model.Entries;
    }

    private void FilterTrackChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (ResultsPanel.DataContext is not ResultsViewModel model) return;
        var layouts = model.Layouts.Where(l => FilterTrack.SelectedIndex == 0 || l.Track == FilterTrack.SelectedItem as string);
        FilterLayout.ItemsSource = new[] { new Choice(0, "Все конфигурации") }.Concat(layouts.Select(l => new Choice(l.Id, l.Name)));
        FilterLayout.SelectedIndex = 0;
    }

    private void FilterResults(object sender, RoutedEventArgs e)
    {
        if (ResultsPanel.DataContext is not ResultsViewModel model) return;
        var ids = model.Layouts.Where(l => FilterTrack.SelectedIndex == 0 || l.Track == FilterTrack.SelectedItem as string).Select(l => l.Id).ToHashSet();
        ResultsGrid.ItemsSource = model.Entries.Where(r => ids.Contains(r.CircuitId)
            && (FilterLayout.SelectedItem is not Choice layout || layout.Id == 0 || r.CircuitId == layout.Id)
            && (FilterCar.SelectedItem is not Choice car || car.Id == 0 || r.CarId == car.Id)).ToArray();
    }

    private void EditTrackChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (ResultsPanel.DataContext is not ResultsViewModel model) return;
        CircuitBox.ItemsSource = model.Layouts.Where(l => l.Track == EditTrackBox.SelectedItem as string);
        CircuitBox.SelectedIndex = 0;
    }

    private void EditResult(object sender, RoutedEventArgs e)
    {
        if (ResultsPanel.DataContext is not ResultsViewModel model) return;
        edited = sender == AddResult ? null : ((sender as FrameworkElement)?.DataContext ?? ResultsGrid.SelectedItem) as ResultEntry;
        if (sender != AddResult && edited == null) { ResultStatus.Text = "Выберите заезд в таблице."; return; }
        EditorTitle.Text = edited == null ? "Добавить результат" : "Изменить результат";
        PilotBox.SelectedValue = edited?.PilotId ?? model.Pilots.FirstOrDefault()?.Id;
        EditTrackBox.ItemsSource = model.Layouts.Select(l => l.Track).Distinct().ToArray();
        var layout = model.Layouts.FirstOrDefault(l => l.Id == edited?.CircuitId) ?? model.Layouts.FirstOrDefault();
        EditTrackBox.SelectedItem = layout?.Track;
        CircuitBox.SelectedValue = layout?.Id;
        CarBox.SelectedValue = edited?.CarId ?? model.Cars.FirstOrDefault()?.Id;
        LapInput.Text = edited?.Time ?? "";
        DateInput.Text = (edited?.DrivenAt ?? DateTime.Now).ToString("dd.MM.yyyy HH:mm:ss");
        ResultStatus.Text = "";
        ResultEditor.Visibility = Visibility.Visible;
    }

    private void CancelResult(object sender, RoutedEventArgs e) => ResultEditor.Visibility = Visibility.Collapsed;

    private async Task ReloadResultsAsync(ResultsViewModel model)
    {
        ResultEditor.Visibility = Visibility.Collapsed;
        await model.LoadAsync();
        ResultsPanel.DataContext = null;
        ResultsPanel.DataContext = model;
            SetResultFilters(model);
        if (DataContext is LeaderboardViewModel board) await board.ShowResultsAsync();
    }

    private async void SaveResult(object sender, RoutedEventArgs e)
    {
        if (ResultsPanel.DataContext is not ResultsViewModel model) return;
        await RunResultsAsync(async () =>
        {
            await model.SaveAsync(edited,PilotBox.SelectedItem as Choice,CircuitBox.SelectedItem is CircuitEntry layout ? new Choice(layout.Id, layout.Name) : null,
                CarBox.SelectedItem as Choice,LapInput.Text,DateInput.Text);
            await ReloadResultsAsync(model);
            ResultStatus.Text = "Результат сохранён.";
        });
    }

    private async void DeleteResult(object sender, RoutedEventArgs e)
    {
        if (ResultsPanel.DataContext is not ResultsViewModel model) return;
        if (((sender as FrameworkElement)?.DataContext ?? ResultsGrid.SelectedItem) is not ResultEntry entry) { ResultStatus.Text = "Выберите заезд в таблице."; return; }
        if (MessageBox.Show(this,$"Удалить заезд: {entry.Pilot}, {entry.Circuit}, {entry.Time}?",
            "Удаление результата",MessageBoxButton.YesNo,MessageBoxImage.Question,MessageBoxResult.No) != MessageBoxResult.Yes) return;
        await RunResultsAsync(async () =>
        {
            await model.DeleteAsync(entry);
            await ReloadResultsAsync(model);
            ResultStatus.Text = "Результат удалён.";
        });
    }
}






