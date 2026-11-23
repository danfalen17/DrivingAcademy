namespace DrivingAcademy.Models;

public record LapResult(string Pilot, string Track, string Configuration, string Car,
                        int Milliseconds, DateOnly Date);
