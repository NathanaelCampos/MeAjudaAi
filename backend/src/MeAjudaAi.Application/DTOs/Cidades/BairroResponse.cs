namespace MeAjudaAi.Application.DTOs.Cidades;

public class BairroResponse
{
    public Guid Id { get; set; }
    public Guid CidadeId { get; set; }
    public string Nome { get; set; } = string.Empty;
}