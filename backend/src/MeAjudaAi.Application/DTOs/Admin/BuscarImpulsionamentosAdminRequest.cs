using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Admin;

public class BuscarImpulsionamentosAdminRequest
{
    public string? Termo { get; set; }
    public Guid? ProfissionalId { get; set; }
    public Guid? PlanoImpulsionamentoId { get; set; }
    public StatusImpulsionamento? Status { get; set; }
    public DateTime? DataInicioInicial { get; set; }
    public DateTime? DataInicioFinal { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
}
