namespace MeAjudaAi.Application.DTOs.Servicos;

public class CriarServicoRequest
{
    public Guid ProfissionalId { get; set; }
    public Guid? ProfissaoId { get; set; }
    public Guid? EspecialidadeId { get; set; }
    public Guid CidadeId { get; set; }
    public Guid? BairroId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal? ValorCombinado { get; set; }
}