using MeAjudaAi.Application.DTOs.Servicos;

namespace MeAjudaAi.Application.Interfaces.Servicos;

public interface IServicoService
{
    Task<ServicoResponse> CriarAsync(
        Guid usuarioId,
        CriarServicoRequest request,
        CancellationToken cancellationToken = default);

    Task<ServicoResponse?> ObterPorIdAsync(
        Guid usuarioId,
        Guid servicoId,
        CancellationToken cancellationToken = default);

    Task<ServicoResponse?> AceitarAsync(
        Guid usuarioId,
        Guid servicoId,
        CancellationToken cancellationToken = default);

    Task<ServicoResponse?> IniciarAsync(
        Guid usuarioId,
        Guid servicoId,
        CancellationToken cancellationToken = default);

    Task<ServicoResponse?> ConcluirAsync(
        Guid usuarioId,
        Guid servicoId,
        CancellationToken cancellationToken = default);

    Task<ServicoResponse?> CancelarAsync(
        Guid usuarioId,
        Guid servicoId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ServicoResponse>> ListarMeusServicosClienteAsync(
        Guid usuarioId,
        ListarServicosRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ServicoResponse>> ListarMeusServicosProfissionalAsync(
        Guid usuarioId,
        ListarServicosRequest request,
        CancellationToken cancellationToken = default);
}