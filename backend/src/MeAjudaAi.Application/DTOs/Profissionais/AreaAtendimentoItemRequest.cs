namespace MeAjudaAi.Application.DTOs.Profissionais;

public class AreaAtendimentoItemRequest
{
    public Guid CidadeId { get; set; }
    public Guid? BairroId { get; set; }
    public bool CidadeInteira { get; set; }
}