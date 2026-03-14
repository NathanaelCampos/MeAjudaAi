using MeAjudaAi.Domain.Common;

namespace MeAjudaAi.Domain.Entities;

public class BackgroundJobFilaAlertaHistorico : EntityBase
{
    public string JobId { get; set; } = string.Empty;
    public string NivelAlerta { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public double TempoMedioFilaSegundos { get; set; }
    public double TempoMedioProcessamentoSegundos { get; set; }
    public int TotalPendentes { get; set; }
    public int TotalFalhas { get; set; }
}
