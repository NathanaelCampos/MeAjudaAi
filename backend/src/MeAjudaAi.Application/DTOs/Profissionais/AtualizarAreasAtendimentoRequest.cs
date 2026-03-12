namespace MeAjudaAi.Application.DTOs.Profissionais;

public class AtualizarAreasAtendimentoRequest
{
    public List<AreaAtendimentoItemRequest> Areas { get; set; } = new();
}