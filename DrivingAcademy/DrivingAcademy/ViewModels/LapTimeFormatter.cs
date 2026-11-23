namespace DrivingAcademy.ViewModels;

internal static class LapTimeFormatter
{
    public static string Format(uint milliseconds)
    {
        var minutes = milliseconds / 60_000;
        var seconds = milliseconds / 1_000 % 60;
        var remainder = milliseconds % 1_000;

        return $"{minutes}:{seconds:00}.{remainder:000}";
    }
}
