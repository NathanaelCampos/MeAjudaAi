using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class EmailNotificacaoResumoOperacionalTipoFalhaResponse
{
    public TipoNotificacao TipoNotificacao { get; set; }
    public int QuantidadeFalhas { get; set; }
}
