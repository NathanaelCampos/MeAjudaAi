using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class BuscarEmailsOutboxRequest
{
    public StatusEmailNotificacao? Status { get; set; }
    public Guid? UsuarioId { get; set; }
    public TipoNotificacao? TipoNotificacao { get; set; }
    public string? EmailDestino { get; set; }
    public DateTime? DataCriacaoInicial { get; set; }
    public DateTime? DataCriacaoFinal { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
}
