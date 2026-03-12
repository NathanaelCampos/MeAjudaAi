using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class PreviewEmailNotificacaoResponse
{
    public TipoNotificacao TipoNotificacao { get; set; }
    public string Assunto { get; set; } = string.Empty;
    public string Html { get; set; } = string.Empty;
    public Guid? ReferenciaId { get; set; }
}
