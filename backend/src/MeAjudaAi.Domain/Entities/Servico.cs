using MeAjudaAi.Domain.Common;
using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Domain.Entities;

public class Servico : EntityBase
{
    public Guid ClienteId { get; set; }
    public Guid ProfissionalId { get; set; }
    public Guid? ProfissaoId { get; set; }
    public Guid? EspecialidadeId { get; set; }
    public Guid CidadeId { get; set; }
    public Guid? BairroId { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal? ValorCombinado { get; set; }

    public StatusServico Status { get; set; } = StatusServico.Solicitado;
    public DateTime? DataAceite { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataConclusao { get; set; }
    public DateTime? DataCancelamento { get; set; }

    public Cliente Cliente { get; set; } = null!;
    public Profissional Profissional { get; set; } = null!;
    public Profissao? Profissao { get; set; }
    public Especialidade? Especialidade { get; set; }
    public Cidade Cidade { get; set; } = null!;
    public Bairro? Bairro { get; set; }

    public Avaliacao? Avaliacao { get; set; }
}