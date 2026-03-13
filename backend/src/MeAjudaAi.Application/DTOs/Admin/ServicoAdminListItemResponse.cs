using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Admin;

public class ServicoAdminListItemResponse
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public Guid ProfissionalId { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public string NomeProfissional { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public StatusServico Status { get; set; }
    public decimal? ValorCombinado { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataConclusao { get; set; }
}
