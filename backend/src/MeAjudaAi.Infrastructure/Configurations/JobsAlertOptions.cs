namespace MeAjudaAi.Infrastructure.Configurations;

public class JobsAlertOptions
{
    public bool Habilitado { get; set; } = true;
    public double TempoEsperaLimiteSegundos { get; set; } = 30;
    public double TempoProcessamentoLimiteSegundos { get; set; } = 30;
}
