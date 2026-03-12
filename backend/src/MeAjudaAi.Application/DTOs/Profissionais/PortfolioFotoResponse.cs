namespace MeAjudaAi.Application.DTOs.Profissionais;

public class PortfolioFotoResponse
{
    public Guid Id { get; set; }
    public string UrlArquivo { get; set; } = string.Empty;
    public string Legenda { get; set; } = string.Empty;
    public int Ordem { get; set; }
}