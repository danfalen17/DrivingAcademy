using System.Windows;
using DrivingAcademy.ViewModels;

namespace DrivingAcademy;

public partial class MainWindow
{
    private Choice? editingCar;

    private async void Cars_Click(object sender, RoutedEventArgs e)
    {
        if (session?.RoleId is not (2 or 3))
            return;
        UsersPanel.Visibility = Visibility.Collapsed;
        TrackTools.Visibility =
            ConfigTools.Visibility =
            ResultTools.Visibility =
                Visibility.Collapsed;

        CarTools.Visibility = Visibility.Visible;
        CarEditor.Visibility = Visibility.Collapsed;

        await RunResultsAsync(async () =>
        {
            var model = new CarsViewModel(session);
            CarTools.DataContext = model;
            CarGrid.ItemsSource = await model.LoadAsync();
        });
    }

    private void EditCar(object sender, RoutedEventArgs e)
    {
        editingCar =
            sender == AddCar
                ? null
                : ((sender as FrameworkElement)?.DataContext ?? CarGrid.SelectedItem) as Choice;
        if (sender != AddCar && editingCar == null)
        {
            ResultStatus.Text = "Выберите автомобиль.";
            return;
        }
        CarName.Text = editingCar?.Label ?? "";
        CarEditorTitle.Text = editingCar == null ? "Добавить запись" : "Изменить запись";

        CarEditor.Visibility = Visibility.Visible;
        ResultStatus.Text = "";
        CarName.Focus();
    }

    private void CancelCar(object sender, RoutedEventArgs e) =>
        CarEditor.Visibility = Visibility.Collapsed;

    private async Task RefreshCarsAsync(CarsViewModel model, string? oldName, string? newName)
    {
        CarEditor.Visibility = Visibility.Collapsed;
        CarGrid.ItemsSource = await model.LoadAsync();
        await RefreshBoardCatalogAsync(oldCar: oldName, newCar: newName);
    }

    private async void SaveCar(object sender, RoutedEventArgs e)
    {
        if (CarTools.DataContext is not CarsViewModel model)
            return;
        await RunResultsAsync(async () =>
        {
            var name = CarName.Text.Trim();
            await model.SaveAsync(editingCar, name);
            await RefreshCarsAsync(model, editingCar?.Label, name);
            ResultStatus.Text = "Автомобиль сохранён.";
        });
    }

    private async void DeleteCar(object sender, RoutedEventArgs e)
    {
        if (CarTools.DataContext is not CarsViewModel model)
            return;
        if (((sender as FrameworkElement)?.DataContext ?? CarGrid.SelectedItem) is not Choice car)
        {
            ResultStatus.Text = "Выберите автомобиль.";
            return;
        }
        if (
            MessageBox.Show(
                this,
                $"Удалить автомобиль «{car.Label}»?",
                "Удаление автомобиля",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No
            ) != MessageBoxResult.Yes
        )
            return;

        await RunResultsAsync(async () =>
        {
            await model.DeleteAsync(car);
            await RefreshCarsAsync(model, car.Label, null);
            ResultStatus.Text = "Автомобиль удалён.";
        });
    }
}
