using MeAjudaAi.Domain.Common;

namespace MeAjudaAi.Domain.Entities;

public class Profissional : EntityBase
{
    public Guid UsuarioId { get; set; }
    public string NomeExibicao { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;

    public string WhatsApp { get; set; } = string.Empty;
    public string Instagram { get; set; } = string.Empty;
    public string Facebook { get; set; } = string.Empty;
    public string OutraFormaContato { get; set; } = string.Empty;

    public bool AceitaContatoPeloApp { get; set; }
    public bool PerfilVerificado { get; set; }

    public decimal? NotaMediaAtendimento { get; set; }
    public decimal? NotaMediaServico { get; set; }
    public decimal? NotaMediaPreco { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public ICollection<AreaAtendimento> AreasAtendimento { get; set; } = new List<AreaAtendimento>();
    public ICollection<ProfissionalProfissao> Profissoes { get; set; } = new List<ProfissionalProfissao>();
    public ICollection<ProfissionalEspecialidade> Especialidades { get; set; } = new List<ProfissionalEspecialidade>();
    public ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();
    public ICollection<PortfolioFoto> PortfolioFotos { get; set; } = new List<PortfolioFoto>();
    public ICollection<FormaRecebimento> FormasRecebimento { get; set; } = new List<FormaRecebimento>();
    public ICollection<ImpulsionamentoProfissional> Impulsionamentos { get; set; } = new List<ImpulsionamentoProfissional>();
    public ICollection<Servico> Servicos { get; set; } = new List<Servico>();
}