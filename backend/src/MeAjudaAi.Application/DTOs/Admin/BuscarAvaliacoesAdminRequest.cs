using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Admin;

public class BuscarAvaliacoesAdminRequest
{
    public string? Termo { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid? ProfissionalId { get; set; }
    public Guid? ServicoId { get; set; }
    public StatusModeracaoComentario? StatusModeracaoComentario { get; set; }
    public DateTime? DataCriacaoInicial { get; set; }
    public DateTime? DataCriacaoFinal { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
}
