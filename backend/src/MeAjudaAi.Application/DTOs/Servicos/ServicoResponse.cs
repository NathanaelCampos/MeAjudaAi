using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Servicos;

public class ServicoResponse
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public Guid ProfissionalId { get; set; }

    public string NomeCliente { get; set; } = string.Empty;
    public string NomeProfissional { get; set; } = string.Empty;

    public Guid? ProfissaoId { get; set; }
    public string? NomeProfissao { get; set; }

    public Guid? EspecialidadeId { get; set; }
    public string? NomeEspecialidade { get; set; }

    public Guid CidadeId { get; set; }
    public string CidadeNome { get; set; } = string.Empty;
    public string UF { get; set; } = string.Empty;

    public Guid? BairroId { get; set; }
    public string? BairroNome { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal? ValorCombinado { get; set; }

    public StatusServico Status { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAceite { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataConclusao { get; set; }
    public DateTime? DataCancelamento { get; set; }
}