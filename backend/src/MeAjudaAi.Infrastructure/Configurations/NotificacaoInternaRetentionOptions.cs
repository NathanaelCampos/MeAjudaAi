namespace MeAjudaAi.Infrastructure.Configurations;

public class NotificacaoInternaRetentionOptions
{
    public bool Habilitada { get; set; }
    public int DiasRetencao { get; set; } = 30;
    public int LoteProcessamento { get; set; } = 500;
    public int IntervaloSegundos { get; set; } = 3600;
    public bool SomenteLidas { get; set; } = true;
}
