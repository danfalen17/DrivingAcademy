using System.Windows;
using DrivingAcademy.ViewModels;

namespace DrivingAcademy;

public partial class MainWindow
{
    private CircuitEntry? editingConfiguration;

    private async void Configurations_Click(object sender, RoutedEventArgs e)
    {
        if (session?.RoleId is not (2 or 3))
            return;
        UsersPanel.Visibility = Visibility.Collapsed;
        CarTools.Visibility =
            ResultTools.Visibility =
            TrackTools.Visibility =
            ConfigEditor.Visibility =
                Visibility.Collapsed;

        ConfigTools.Visibility = Visibility.Visible;
        ConfigTools.DataContext = null;
        ConfigGrid.ItemsSource = null;

        await RunResultsAsync(async () =>
        {
            var model = new ConfigurationsViewModel(session);
            ParentTrack.ItemsSource = await new TracksViewModel(session).LoadAsync();
            ConfigGrid.ItemsSource = await model.LoadAsync();
            ConfigTools.DataContext = model;
        });
    }

    private void EditConfiguration(object sender, RoutedEventArgs e)
    {
        if (ConfigTools.DataContext is not ConfigurationsViewModel)
            return;
        editingConfiguration =
            sender == AddConfig
                ? null
                : ((sender as FrameworkElement)?.DataContext ?? ConfigGrid.SelectedItem)
                    as CircuitEntry;
        if (sender != AddConfig && editingConfiguration == null)
        {
            ResultStatus.Text = "Выберите конфигурацию.";
            return;
        }
        if (editingConfiguration == null)
            ParentTrack.SelectedIndex = 0;
        else
            ParentTrack.SelectedValue = editingConfiguration.TrackId;
        ConfigName.Text = editingConfiguration?.Name ?? "";
        ConfigEditorTitle.Text =
            editingConfiguration == null ? "Добавить запись" : "Изменить запись";

        ConfigEditor.Visibility = Visibility.Visible;
        ResultStatus.Text = "";
        ConfigName.Focus();
    }

    private void CancelConfiguration(object sender, RoutedEventArgs e) =>
        ConfigEditor.Visibility = Visibility.Collapsed;

    private async Task RefreshConfigurationsAsync(ConfigurationsViewModel model)
    {
        ConfigEditor.Visibility = Visibility.Collapsed;
        ConfigGrid.ItemsSource = await model.LoadAsync();
        await RefreshBoardCatalogAsync();
    }

    private async void SaveConfiguration(object sender, RoutedEventArgs e)
    {
        if (ConfigTools.DataContext is not ConfigurationsViewModel model)
            return;

        await RunResultsAsync(async () =>
        {
            await model.SaveAsync(
                editingConfiguration,
                ParentTrack.SelectedItem as Choice,
                ConfigName.Text
            );
            await RefreshConfigurationsAsync(model);
            ResultStatus.Text = "Конфигурация сохранена.";
        });
    }

    private async void DeleteConfiguration(object sender, RoutedEventArgs e)
    {
        if (ConfigTools.DataContext is not ConfigurationsViewModel model)
            return;

        if (
            ((sender as FrameworkElement)?.DataContext ?? ConfigGrid.SelectedItem)
            is not CircuitEntry entry
        )
        {
            ResultStatus.Text = "Выберите конфигурацию.";
            return;
        }

        if (
            MessageBox.Show(
                this,
                $"Удалить конфигурацию «{entry.Name}» трассы «{entry.Track}»?",
                "Удаление конфигурации",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No
            ) != MessageBoxResult.Yes
        )
            return;

        await RunResultsAsync(async () =>
        {
            await model.DeleteAsync(entry);
            await RefreshConfigurationsAsync(model);
            ResultStatus.Text = "Конфигурация удалена.";
        });
    }
}
