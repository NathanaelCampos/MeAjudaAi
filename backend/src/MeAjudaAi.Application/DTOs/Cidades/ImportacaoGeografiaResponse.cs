namespace MeAjudaAi.Application.DTOs.Cidades;

public class ImportacaoGeografiaResponse
{
    public string Mensagem { get; set; } = string.Empty;
    public int Estados { get; set; }
    public int Cidades { get; set; }
    public int Bairros { get; set; }
}
