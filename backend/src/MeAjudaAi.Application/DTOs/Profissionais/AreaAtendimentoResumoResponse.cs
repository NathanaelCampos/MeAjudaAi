namespace MeAjudaAi.Application.DTOs.Profissionais;

public class AreaAtendimentoResumoResponse
{
    public Guid CidadeId { get; set; }
    public string CidadeNome { get; set; } = string.Empty;
    public string UF { get; set; } = string.Empty;
    public Guid? BairroId { get; set; }
    public string? BairroNome { get; set; }
    public bool CidadeInteira { get; set; }
}