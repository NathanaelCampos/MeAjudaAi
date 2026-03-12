using MeAjudaAi.Domain.Common;
using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Domain.Entities;

public class EmailNotificacaoOutbox : EntityBase
{
    public Guid UsuarioId { get; set; }
    public TipoNotificacao TipoNotificacao { get; set; }
    public string EmailDestino { get; set; } = string.Empty;
    public string Assunto { get; set; } = string.Empty;
    public string Corpo { get; set; } = string.Empty;
    public Guid? ReferenciaId { get; set; }
    public StatusEmailNotificacao Status { get; set; } = StatusEmailNotificacao.Pendente;
    public int TentativasProcessamento { get; set; }
    public DateTime? DataProcessamento { get; set; }
    public string UltimaMensagemErro { get; set; } = string.Empty;

    public Usuario Usuario { get; set; } = null!;
}
