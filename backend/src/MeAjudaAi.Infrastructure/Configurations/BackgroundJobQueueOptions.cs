namespace MeAjudaAi.Infrastructure.Configurations;

public class BackgroundJobQueueOptions
{
    public bool Habilitada { get; set; }
    public int IntervaloSegundos { get; set; } = 30;
    public int LoteProcessamento { get; set; } = 20;
}
