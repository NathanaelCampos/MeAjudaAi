namespace MeAjudaAi.Application.DTOs.Profissionais;

public class ProfissionalResumoResponse
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string NomeExibicao { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public bool AceitaContatoPeloApp { get; set; }
    public bool PerfilVerificado { get; set; }
    public bool EstaImpulsionado { get; set; }
    public decimal? NotaMediaAtendimento { get; set; }
    public decimal? NotaMediaServico { get; set; }
    public decimal? NotaMediaPreco { get; set; }

    public List<ProfissaoResumoResponse> Profissoes { get; set; } = new();
    public List<EspecialidadeResumoResponse> Especialidades { get; set; } = new();
    public List<AreaAtendimentoResumoResponse> AreasAtendimento { get; set; } = new();
}