using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class ArquivarNotificacoesEmLoteRequest
{
    public Guid? UsuarioId { get; set; }
    public TipoNotificacao? TipoNotificacao { get; set; }
    public bool? Lida { get; set; }
    public DateTime? DataCriacaoInicial { get; set; }
    public DateTime? DataCriacaoFinal { get; set; }
    public int Limite { get; set; } = 100;
}
