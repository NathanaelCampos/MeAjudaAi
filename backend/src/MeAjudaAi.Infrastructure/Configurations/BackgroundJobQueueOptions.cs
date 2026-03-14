namespace MeAjudaAi.Infrastructure.Configurations;

public class BackgroundJobQueueOptions
{
    public bool Habilitada { get; set; }
    public int IntervaloSegundos { get; set; } = 30;
    public int LoteProcessamento { get; set; } = 20;
    public int MaxTentativas { get; set; } = 3;
    public int AtrasoBaseSegundos { get; set; } = 60;
}
