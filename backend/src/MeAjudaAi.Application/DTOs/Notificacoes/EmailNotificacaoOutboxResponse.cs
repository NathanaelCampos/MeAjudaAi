using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class EmailNotificacaoOutboxResponse
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public TipoNotificacao TipoNotificacao { get; set; }
    public string EmailDestino { get; set; } = string.Empty;
    public string Assunto { get; set; } = string.Empty;
    public string Corpo { get; set; } = string.Empty;
    public Guid? ReferenciaId { get; set; }
    public StatusEmailNotificacao Status { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataProcessamento { get; set; }
    public string UltimaMensagemErro { get; set; } = string.Empty;
}
