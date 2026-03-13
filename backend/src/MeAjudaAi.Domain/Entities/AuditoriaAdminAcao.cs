using MeAjudaAi.Domain.Common;

namespace MeAjudaAi.Domain.Entities;

public class AuditoriaAdminAcao : EntityBase
{
    public Guid AdminUsuarioId { get; set; }
    public Usuario AdminUsuario { get; set; } = null!;
    public string Entidade { get; set; } = string.Empty;
    public Guid EntidadeId { get; set; }
    public string Acao { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
}
