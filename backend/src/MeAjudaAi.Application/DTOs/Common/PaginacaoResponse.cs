namespace MeAjudaAi.Application.DTOs.Common;

public class PaginacaoResponse<T>
{
    public int PaginaAtual { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalRegistros { get; set; }
    public int TotalPaginas { get; set; }
    public IReadOnlyList<T> Itens { get; set; } = Array.Empty<T>();
}