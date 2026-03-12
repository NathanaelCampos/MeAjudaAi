using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class AtualizarEmailsOutboxEmLoteRequest
{
    public StatusEmailNotificacao? Status { get; set; }
    public Guid? UsuarioId { get; set; }
    public TipoNotificacao? TipoNotificacao { get; set; }
    public string? EmailDestino { get; set; }
    public DateTime? DataCriacaoInicial { get; set; }
    public DateTime? DataCriacaoFinal { get; set; }
    public int Limite { get; set; } = 100;
}
