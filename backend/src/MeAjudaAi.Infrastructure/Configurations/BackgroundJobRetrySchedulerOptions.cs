namespace MeAjudaAi.Infrastructure.Configurations;

public class BackgroundJobRetrySchedulerOptions
{
    public bool Habilitado { get; set; } = true;
    public int IntervaloSegundos { get; set; } = 120;
    public int FalhasParaRetentar { get; set; } = 1;
    public int TempoMaximoDesdeFalhaSegundos { get; set; } = 300;
}
