using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class EmailNotificacaoResumoOperacionalResponse
{
    public Guid? UsuarioId { get; set; }
    public TipoNotificacao? TipoNotificacao { get; set; }
    public string? EmailDestino { get; set; }
    public DateTime? DataCriacaoInicial { get; set; }
    public DateTime? DataCriacaoFinal { get; set; }
    public int TotalRegistros { get; set; }
    public int Pendentes { get; set; }
    public int Enviados { get; set; }
    public int Falhas { get; set; }
    public int Cancelados { get; set; }
    public int ProntosParaProcessar { get; set; }
    public int AguardandoProximaTentativa { get; set; }
    public IReadOnlyList<EmailNotificacaoResumoOperacionalTipoFalhaResponse> TopTiposComFalha { get; set; } =
        Array.Empty<EmailNotificacaoResumoOperacionalTipoFalhaResponse>();
    public IReadOnlyList<EmailNotificacaoResumoOperacionalDestinatarioFalhaResponse> TopDestinatariosComFalha { get; set; } =
        Array.Empty<EmailNotificacaoResumoOperacionalDestinatarioFalhaResponse>();
}
