using MeAjudaAi.Domain.Common;
using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Domain.Entities;

public class BackgroundJobExecucao : EntityBase
{
    public string JobId { get; set; } = string.Empty;
    public string NomeJob { get; set; } = string.Empty;
    public string Origem { get; set; } = string.Empty;
    public Guid? SolicitadoPorAdminUsuarioId { get; set; }
    public Usuario? SolicitadoPorAdminUsuario { get; set; }
    public StatusExecucaoBackgroundJob Status { get; set; } = StatusExecucaoBackgroundJob.Pendente;
    public int TentativasProcessamento { get; set; }
    public int RegistrosProcessados { get; set; }
    public DateTime? ProcessarAposUtc { get; set; }
    public DateTime? DataInicioProcessamento { get; set; }
    public DateTime? DataFinalizacao { get; set; }
    public string MensagemResultado { get; set; } = string.Empty;
}
