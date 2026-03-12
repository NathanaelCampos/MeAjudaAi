using MeAjudaAi.Domain.Common;
using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Domain.Entities;

public class NotificacaoUsuario : EntityBase
{
    public Guid UsuarioId { get; set; }
    public TipoNotificacao Tipo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public Guid? ReferenciaId { get; set; }
    public DateTime? DataLeitura { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
