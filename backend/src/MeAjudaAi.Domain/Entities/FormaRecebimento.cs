using MeAjudaAi.Domain.Common;
using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Domain.Entities;

public class FormaRecebimento : EntityBase
{
    public Guid ProfissionalId { get; set; }
    public TipoFormaRecebimento TipoFormaRecebimento { get; set; }
    public string Descricao { get; set; } = string.Empty;

    public Profissional Profissional { get; set; } = null!;
}