using MeAjudaAi.Application.DTOs.Notificacoes;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.Interfaces.Notificacoes;

public interface INotificacaoService
{
    Task CriarAsync(
        Guid usuarioId,
        TipoNotificacao tipo,
        string titulo,
        string mensagem,
        Guid? referenciaId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificacaoResponse>> ListarMinhasAsync(
        Guid usuarioId,
        bool somenteNaoLidas = false,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PreferenciaNotificacaoResponse>> ListarPreferenciasAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PreferenciaNotificacaoResponse>> AtualizarPreferenciasAsync(
        Guid usuarioId,
        IReadOnlyList<PreferenciaNotificacaoItemRequest> preferencias,
        CancellationToken cancellationToken = default);

    Task<PaginacaoResponse<EmailNotificacaoOutboxResponse>> ListarEmailsOutboxAsync(
        BuscarEmailsOutboxRequest request,
        CancellationToken cancellationToken = default);

    Task<string> ExportarEmailsOutboxCsvAsync(
        ExportarEmailsOutboxRequest request,
        CancellationToken cancellationToken = default);

    Task<EmailNotificacaoOutboxResponse?> ObterEmailOutboxPorIdAsync(
        Guid emailId,
        CancellationToken cancellationToken = default);

    Task<EmailNotificacaoOutboxResponse?> CancelarEmailOutboxAsync(
        Guid emailId,
        CancellationToken cancellationToken = default);

    Task<EmailNotificacaoOutboxResponse?> ReabrirEmailOutboxAsync(
        Guid emailId,
        CancellationToken cancellationToken = default);

    Task<int> CancelarEmailsOutboxEmLoteAsync(
        AtualizarEmailsOutboxEmLoteRequest request,
        CancellationToken cancellationToken = default);

    Task<int> ReabrirEmailsOutboxEmLoteAsync(
        AtualizarEmailsOutboxEmLoteRequest request,
        CancellationToken cancellationToken = default);

    Task<int> ReprocessarEmailsOutboxEmLoteAsync(
        AtualizarEmailsOutboxEmLoteRequest request,
        CancellationToken cancellationToken = default);

    Task<int> ReprocessarEmailsOutboxAsync(
        CancellationToken cancellationToken = default);

    Task<EmailNotificacaoMetricasResponse> ObterMetricasEmailsOutboxAsync(
        BuscarMetricasEmailsOutboxRequest request,
        CancellationToken cancellationToken = default);

    Task<EmailNotificacaoMetricasSerieResponse> ObterMetricasSerieEmailsOutboxAsync(
        BuscarMetricasEmailsOutboxRequest request,
        CancellationToken cancellationToken = default);

    Task<EmailNotificacaoDestinatariosMetricasResponse> ObterMetricasDestinatariosEmailsOutboxAsync(
        BuscarMetricasEmailsOutboxRequest request,
        CancellationToken cancellationToken = default);

    Task<EmailNotificacaoTiposMetricasResponse> ObterMetricasTiposEmailsOutboxAsync(
        BuscarMetricasEmailsOutboxRequest request,
        CancellationToken cancellationToken = default);

    Task<EmailNotificacaoDashboardResponse> ObterDashboardEmailsOutboxAsync(
        BuscarMetricasEmailsOutboxRequest request,
        CancellationToken cancellationToken = default);

    Task<EmailNotificacaoUsuarioDashboardResponse> ObterDashboardEmailsOutboxPorUsuarioAsync(
        Guid usuarioId,
        BuscarMetricasEmailsOutboxRequest request,
        CancellationToken cancellationToken = default);

    Task<QuantidadeNotificacoesNaoLidasResponse> ObterQuantidadeNaoLidasAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    Task<NotificacaoResponse?> MarcarComoLidaAsync(
        Guid usuarioId,
        Guid notificacaoId,
        CancellationToken cancellationToken = default);

    Task<int> MarcarTodasComoLidasAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);
}
