namespace MeAjudaAi.Infrastructure.Configurations;

public class EmailNotificacaoOptions
{
    public bool ProcessadorHabilitado { get; set; }
    public int LoteProcessamento { get; set; } = 20;
    public int IntervaloSegundos { get; set; } = 60;
    public int AtrasoBaseSegundos { get; set; } = 60;
    public int MaxTentativas { get; set; } = 3;
    public bool SimularEnvio { get; set; } = true;
}
