using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Admin;

public class ServicoAdminDashboardAvaliacaoResponse
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public Guid ProfissionalId { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public NotaAtendimento NotaAtendimento { get; set; }
    public NotaServico NotaServico { get; set; }
    public NotaPreco NotaPreco { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public StatusModeracaoComentario StatusModeracaoComentario { get; set; }
    public DateTime DataCriacao { get; set; }
}
