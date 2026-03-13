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

    Task<PaginacaoResponse<NotificacaoAdminResponse>> ListarNotificacoesAsync(
        BuscarNotificacoesRequest request,
        CancellationToken cancellationToken = default);

    Task<PaginacaoResponse<NotificacaoAdminResponse>> ListarNotificacoesArquivadasAsync(
        BuscarNotificacoesRequest request,
        CancellationToken cancellationToken = default);

    Task<string> ExportarNotificacoesCsvAsync(
        ExportarNotificacoesRequest request,
        CancellationToken cancellationToken = default);

    Task<string> ExportarNotificacoesArquivadasCsvAsync(
        ExportarNotificacoesRequest request,
        CancellationToken cancellationToken = default);

    Task<NotificacaoAdminResponse?> ObterNotificacaoPorIdAsync(
        Guid notificacaoId,
        CancellationToken cancellationToken = default);

    Task<NotificacaoAdminResponse?> ObterNotificacaoArquivadaPorIdAsync(
        Guid notificacaoId,
        CancellationToken cancellationToken = default);

    Task<int> MarcarNotificacoesComoLidasEmLoteAsync(
        MarcarNotificacoesComoLidasEmLoteRequest request,
        CancellationToken cancellationToken = default);

    Task<int> ArquivarNotificacoesEmLoteAsync(
        ArquivarNotificacoesEmLoteRequest request,
        CancellationToken cancellationToken = default);

    Task<int> RestaurarNotificacoesEmLoteAsync(
        ArquivarNotificacoesEmLoteRequest request,
        CancellationToken cancellationToken = default);

    Task<int> ExcluirNotificacoesArquivadasEmLoteAsync(
        ArquivarNotificacoesEmLoteRequest request,
        CancellationToken cancellationToken = default);

    Task<PreviewArquivamentoNotificacoesResponse> PreviewArquivamentoNotificacoesAsync(
        ArquivarNotificacoesEmLoteRequest request,
        CancellationToken cancellationToken = default);

    Task<PreviewArquivamentoNotificacoesResponse> PreviewRestauracaoNotificacoesAsync(
        ArquivarNotificacoesEmLoteRequest request,
        CancellationToken cancellationToken = default);

    Task<NotificacaoResumoOperacionalResponse> ObterResumoOperacionalNotificacoesAsync(
        Guid? usuarioId = null,
        TipoNotificacao? tipoNotificacao = null,
        DateTime? dataCriacaoInicial = null,
        DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default);

    Task<NotificacaoResumoOperacionalResponse> ObterResumoOperacionalNotificacoesArquivadasAsync(
        Guid? usuarioId = null,
        TipoNotificacao? tipoNotificacao = null,
        DateTime? dataCriacaoInicial = null,
        DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default);

    Task<NotificacaoUsuarioDashboardResponse> ObterDashboardNotificacoesPorUsuarioAsync(
        Guid usuarioId,
        TipoNotificacao? tipoNotificacao = null,
        DateTime? dataCriacaoInicial = null,
        DateTime? dataCriacaoFinal = null,
        CancellationToken cancellationToken = default);

    Task<NotificacaoUsuarioDashboardResponse> ObterDashboardNotificacoesArquivadasPorUsuarioAsync(
        Guid usuarioId,
        TipoNotificacao? tipoNotificacao = null,
        DateTime? dataCriacaoInicial = null,
        DateTime? dataCriacaoFinal = null,
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

    Task<EmailNotificacaoResumoOperacionalResponse> ObterResumoOperacionalEmailsOutboxAsync(
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
