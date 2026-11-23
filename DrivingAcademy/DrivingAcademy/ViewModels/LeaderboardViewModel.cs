using DrivingAcademy.Data;
using MySqlConnector;
using System.ComponentModel;
using System.Windows.Input;

namespace DrivingAcademy.ViewModels;

public record LeaderboardRow(int Place, string Pilot, string BestLap, DateOnly Date);

internal record CarFilter(string? Name)
{
    public string Label => Name ?? "Все автомобили";
}

internal class LeaderboardViewModel : INotifyPropertyChanged
{
    private readonly TrackConfiguration[] configurations;
    private string? selectedTrack;
    public string[] Tracks { get; }
    public CarFilter[] Cars { get; }
    public string[] Configurations => configurations.Where(c => c.Track == SelectedTrack).Select(c => c.Name).ToArray();
    public string? SelectedTrack
    {
        get => selectedTrack;
        set
        {
            if (selectedTrack == value) return;
            selectedTrack = value;
            Notify(nameof(Configurations));
            SelectedConfiguration = Configurations.FirstOrDefault();
            Notify(nameof(SelectedConfiguration));
        }
    }
    public string? SelectedConfiguration { get; set; }
    public CarFilter? SelectedCar { get; set; }
    public LeaderboardRow[] Rows { get; private set; } = [];
    public string Status { get; private set; } = "";
    public ICommand ShowCommand { get; }
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Notify(string name) => PropertyChanged?.Invoke(this, new(name));

    public LeaderboardViewModel(string[] tracks, TrackConfiguration[] configurations, string[] cars)
    {
        Tracks = tracks;
        Cars = [new CarFilter(null), .. cars.Select(name => new CarFilter(name))];
        this.configurations = configurations;
        SelectedTrack = tracks.FirstOrDefault();
        SelectedCar = Cars[0];
        ShowCommand = new ShowResultsCommand(ShowResultsAsync);
    }

    private int requestNumber;
    public async Task ShowResultsAsync()
    {
        int request = ++requestNumber;
        var track = SelectedTrack;
        var configuration = SelectedConfiguration;
        var car = SelectedCar;
        Rows = [];
        Notify(nameof(Rows));
        Status = "Выберите трассу, конфигурацию и автомобиль.";
        if (track != null && configuration != null && car != null)
        {
            Status = "Загрузка результатов…";
            Notify(nameof(Status));
            try
            {
                using var database = new AcademyContext();
                var laps = await database.LoadRecordsAsync(track, configuration, car.Name);
                if (request != requestNumber) return;
                Rows = laps.Select((lap, index) => new LeaderboardRow(index + 1, lap.Pilot,
                    $"{lap.Milliseconds / 60000}:{lap.Milliseconds / 1000 % 60:00}.{lap.Milliseconds % 1000:000}",
                    DateOnly.FromDateTime(lap.DrivenAt))).ToArray();
                Notify(nameof(Rows));
                Status = $"{track} · {configuration} · {car.Label}";
            }
            catch (Exception error) when (error is MySqlException or InvalidOperationException or ArgumentException)
            {
                if (request != requestNumber) return;
                Status = "Не удалось загрузить результаты. Проверьте подключение и нажмите «Показать».";
            }
        }
        Notify(nameof(Status));
    }
}

internal class ShowResultsCommand(Func<Task> execute) : ICommand
{
    public bool CanExecute(object? parameter) => true;
    public async void Execute(object? parameter) => await execute();
    public event EventHandler? CanExecuteChanged { add { } remove { } }
}

