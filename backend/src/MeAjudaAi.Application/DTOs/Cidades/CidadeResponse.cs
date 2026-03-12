namespace MeAjudaAi.Application.DTOs.Cidades;

public class CidadeResponse
{
    public Guid Id { get; set; }
    public Guid EstadoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string UF { get; set; } = string.Empty;
    public string CodigoIbge { get; set; } = string.Empty;
}