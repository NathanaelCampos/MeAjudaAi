namespace MeAjudaAi.Application.DTOs.Profissionais;

public class AtualizarPortfolioRequest
{
    public List<PortfolioFotoRequest> Fotos { get; set; } = new();
}