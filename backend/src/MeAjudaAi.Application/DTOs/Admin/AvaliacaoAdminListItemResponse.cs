using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Admin;

public class AvaliacaoAdminListItemResponse
{
    public Guid Id { get; set; }
    public Guid ServicoId { get; set; }
    public Guid ClienteId { get; set; }
    public Guid ProfissionalId { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public string NomeProfissional { get; set; } = string.Empty;
    public string Comentario { get; set; } = string.Empty;
    public StatusModeracaoComentario StatusModeracaoComentario { get; set; }
    public DateTime DataCriacao { get; set; }
}
