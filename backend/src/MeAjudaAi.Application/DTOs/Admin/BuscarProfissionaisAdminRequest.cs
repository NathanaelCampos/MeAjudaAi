namespace MeAjudaAi.Application.DTOs.Admin;

public class BuscarProfissionaisAdminRequest
{
    public string? Nome { get; set; }
    public bool? Ativo { get; set; }
    public bool? PerfilVerificado { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
}
