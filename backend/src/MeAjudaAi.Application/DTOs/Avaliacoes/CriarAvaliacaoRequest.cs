using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Avaliacoes;

public class CriarAvaliacaoRequest
{
    public Guid ServicoId { get; set; }
    public NotaAtendimento NotaAtendimento { get; set; }
    public NotaServico NotaServico { get; set; }
    public NotaPreco NotaPreco { get; set; }
    public string Comentario { get; set; } = string.Empty;
}