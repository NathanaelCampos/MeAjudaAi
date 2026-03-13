namespace MeAjudaAi.Application.DTOs.Admin;

public class BuscarAuditoriasAdminRequest
{
    public Guid? AdminUsuarioId { get; set; }
    public string? Entidade { get; set; }
    public Guid? EntidadeId { get; set; }
    public string? Acao { get; set; }
    public DateTime? DataCriacaoInicial { get; set; }
    public DateTime? DataCriacaoFinal { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
}
