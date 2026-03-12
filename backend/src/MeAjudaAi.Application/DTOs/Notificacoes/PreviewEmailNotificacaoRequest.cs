using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class PreviewEmailNotificacaoRequest
{
    public TipoNotificacao TipoNotificacao { get; set; }
    public string Assunto { get; set; } = string.Empty;
    public string Corpo { get; set; } = string.Empty;
    public Guid? ReferenciaId { get; set; }
}
