using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.DTOs.Profissionais;

namespace MeAjudaAi.Application.Interfaces.Profissionais;

public interface IProfissionalService
{
    Task<IReadOnlyList<ProfissionalResponse>> ListarAsync(
        ListarProfissionaisRequest request,
        CancellationToken cancellationToken = default);

    Task<PaginacaoResponse<ProfissionalResumoResponse>> BuscarAsync(
        BuscarProfissionaisRequest request,
        CancellationToken cancellationToken = default);

    Task<ProfissionalResponse?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ProfissionalDetalhesResponse?> ObterDetalhesPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ProfissionalResponse?> AtualizarAsync(
        Guid id,
        AtualizarProfissionalRequest request,
        CancellationToken cancellationToken = default);

    Task<ProfissionalResponse?> AtualizarPorUsuarioIdAsync(
        Guid usuarioId,
        AtualizarProfissionalRequest request,
        CancellationToken cancellationToken = default);

    Task AtualizarProfissoesAsync(
        Guid usuarioId,
        AtualizarProfissoesProfissionalRequest request,
        CancellationToken cancellationToken = default);

    Task AtualizarEspecialidadesAsync(
        Guid usuarioId,
        AtualizarEspecialidadesProfissionalRequest request,
        CancellationToken cancellationToken = default);

    Task AtualizarAreasAtendimentoAsync(
        Guid usuarioId,
        AtualizarAreasAtendimentoRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PortfolioFotoResponse>> ListarPortfolioAsync(
        Guid profissionalId,
        CancellationToken cancellationToken = default);

    Task AtualizarPortfolioAsync(
        Guid usuarioId,
        AtualizarPortfolioRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FormaRecebimentoResponse>> ListarFormasRecebimentoAsync(
        Guid profissionalId,
        CancellationToken cancellationToken = default);

    Task AtualizarFormasRecebimentoAsync(
        Guid usuarioId,
        AtualizarFormasRecebimentoRequest request,
        CancellationToken cancellationToken = default);

    Task<UploadPortfolioResponse> UploadPortfolioAsync(
Guid usuarioId,
Stream stream,
string nomeArquivoOriginal,
string contentType,
long tamanhoArquivo,
CancellationToken cancellationToken = default);
}