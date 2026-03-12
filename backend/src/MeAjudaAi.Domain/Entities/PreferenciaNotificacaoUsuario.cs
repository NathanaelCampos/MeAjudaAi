using MeAjudaAi.Domain.Common;
using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Domain.Entities;

public class PreferenciaNotificacaoUsuario : EntityBase
{
    public Guid UsuarioId { get; set; }
    public TipoNotificacao Tipo { get; set; }
    public bool AtivoInterno { get; set; } = true;
    public bool AtivoEmail { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
