using MeAjudaAi.Domain.Common;

namespace MeAjudaAi.Domain.Entities;

public class Cliente : EntityBase
{
    public Guid UsuarioId { get; set; }
    public string NomeExibicao { get; set; } = string.Empty;

    public Usuario Usuario { get; set; } = null!;
    public ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();
    public ICollection<Servico> Servicos { get; set; } = new List<Servico>();
}