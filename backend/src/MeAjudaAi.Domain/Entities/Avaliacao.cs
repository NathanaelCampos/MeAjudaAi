using MeAjudaAi.Domain.Common;
using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Domain.Entities;

public class Avaliacao : EntityBase
{
    public Guid ServicoId { get; set; }
    public Guid ClienteId { get; set; }
    public Guid ProfissionalId { get; set; }

    public NotaAtendimento NotaAtendimento { get; set; }
    public NotaServico NotaServico { get; set; }
    public NotaPreco NotaPreco { get; set; }

    public string Comentario { get; set; } = string.Empty;
    public StatusModeracaoComentario StatusModeracaoComentario { get; set; } = StatusModeracaoComentario.Pendente;

    public Servico Servico { get; set; } = null!;
    public Cliente Cliente { get; set; } = null!;
    public Profissional Profissional { get; set; } = null!;
}