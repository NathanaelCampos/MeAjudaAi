namespace MeAjudaAi.Infrastructure.Configurations;

public class BackgroundJobWatchdogOptions
{
    public bool Habilitado { get; set; } = true;
    public int IntervaloSegundos { get; set; } = 60;
    public int TempoMaximoProcessandoSegundos { get; set; } = 300;
}
